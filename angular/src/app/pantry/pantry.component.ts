import { Component, Inject, OnInit, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreModule, ListService, LocalizationPipe } from '@abp/ng.core';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { PantryItemService, PantryItemDto, CreateUpdatePantryItemDto } from '@proxy/pantry-items';
import { MatDialog, MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

export function fechaValidaValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value || typeof value !== 'string') {
    return { invalidDate: true };
  }
  const timestamp = Date.parse(value);
  if (isNaN(timestamp)) {
    return { invalidDate: true };
  }
  return null;
}

@Component({
  selector: 'app-pantry',
  standalone: true,
  imports: [CommonModule, CoreModule],
  providers: [ListService],
  templateUrl: './pantry.component.html'
})
export class PantryComponent implements OnInit {
  itemsDisponibles: PantryItemDto[] = [];
  itemsConsumidos: PantryItemDto[] = [];

  hasError = false;

  constructor(
    public readonly list: ListService,
    private pantryService: PantryItemService,
    private dialog: MatDialog,
    private confirmation: ConfirmationService
  ) { }

  ngOnInit() {
    const streamCreator = (query: any) => this.pantryService.getList(query, { skipHandleError: true });

    this.list.hookToQuery(streamCreator).subscribe({
      next: (response) => {
        const todosLosItems = response.items || [];
        this.itemsDisponibles = todosLosItems.filter(x => x.estado === 0);
        this.itemsConsumidos = todosLosItems.filter(x => x.estado === 1);
        this.hasError = false;
      },
      error: () => {
        this.hasError = true;
      }
    });
  }

  reintentar() {
    this.hasError = false;
    this.list.get();
  }

  eliminar(item: PantryItemDto): void {
    this.confirmation
      .warn('::Pantry:DeleteConfirmMessage', '::Pantry:DeleteConfirmTitle')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm && item.id) {
          this.pantryService.delete(item.id).subscribe(() => {
            this.list.get();
          });
        }
      });
  }

  abrirModal(item: PantryItemDto): void {
    const dialogRef = this.dialog.open(MiModalComponent, {
      width: '400px',
      data: {
        item,
        cantidad: item.cantidad,
        unidad: item.unidad,
        fechaven: item.fechaVencimiento,
        estado: item.estado
      }
    });

    dialogRef.afterClosed().subscribe((resultado: any) => {
      if (!resultado) {
        return;
      }
      if (resultado.updated) {
        this.list.get();
        return;
      }
      if (item.id) {
        let fechaFormateada = resultado.fechaven;
        if (fechaFormateada) {
          fechaFormateada = new Date(resultado.fechaven).toISOString();
        }
        const nuevoEstado = resultado.esConsumido ? 1 : 0;
        const datosActualizados: CreateUpdatePantryItemDto = {
          idPropietario: item.idPropietario!,
          nombreProducto: item.nombreProducto!,
          cantidad: resultado.cantidad,
          unidad: resultado.unidad,
          fechaVencimiento: fechaFormateada,
          estado: nuevoEstado
        };

        this.pantryService.update(item.id, datosActualizados).subscribe(() => {
          this.list.get();
        });
      }
    });
  }
}

@Component({
  selector: 'app-mi-modal',
  standalone: true,
  imports: [CommonModule, LocalizationPipe, ReactiveFormsModule, MatDialogModule, MatInputModule, MatButtonModule],
  templateUrl: './editpantry.component.html'
})
export class MiModalComponent implements OnInit {
  miFormulario!: FormGroup;
  isSaving = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<MiModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    @Optional() private pantryService?: PantryItemService
  ) { }

  ngOnInit(): void {
    let fechaFormateada = '';
    if (this.data?.fechaven) {
      fechaFormateada = this.data.fechaven.substring(0, 10);
    }
    const yaEstaConsumido = this.data?.estado === 1;
    this.miFormulario = this.fb.group({
      cantidad: [this.data?.cantidad ?? '', [Validators.required, Validators.min(0)]],
      unidad: [this.data?.unidad ?? '', [Validators.required, Validators.minLength(1), Validators.maxLength(32)]],
      fechaven: [fechaFormateada, [Validators.required, fechaValidaValidator]],
      esConsumido: [yaEstaConsumido]
    });
  }

  guardar(): void {
    if (this.miFormulario.invalid || this.isSaving) {
      this.miFormulario.markAllAsTouched();
      return;
    }

    const item: PantryItemDto | undefined = this.data?.item;
    if (item?.id && this.pantryService) {
      this.isSaving = true;
      this.errorMessage = '';

      const formVal = this.miFormulario.value;
      const fechaFormateada = formVal.fechaven ? new Date(formVal.fechaven).toISOString() : null;
      const datosActualizados: CreateUpdatePantryItemDto = {
        idPropietario: item.idPropietario!,
        nombreProducto: item.nombreProducto!,
        cantidad: formVal.cantidad,
        unidad: formVal.unidad,
        fechaVencimiento: fechaFormateada,
        estado: formVal.esConsumido ? 1 : 0
      };

      this.pantryService.update(item.id, datosActualizados, { skipHandleError: true }).subscribe({
        next: () => {
          this.isSaving = false;
          this.dialogRef.close({ updated: true });
        },
        error: () => {
          this.isSaving = false;
          this.errorMessage = '::Pantry:UpdateError';
        }
      });
    } else {
      this.dialogRef.close(this.miFormulario.value);
    }
  }

  cancelar(): void {
    this.dialogRef.close();
  }
}
