import { Component, Inject, OnInit, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreModule, ListService, LocalizationPipe } from '@abp/ng.core';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { PantryItemService, PantryItemDto, CreateUpdatePantryItemDto } from '@proxy/pantry-items';
import { MatDialog, MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { catchError, throwError } from 'rxjs';

export function fechaValidaValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (value === null || value === undefined || value === '') {
    return null;
  }
  if (typeof value !== 'string') {
    return { invalidDate: true };
  }
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value.trim());
  if (!match) {
    return { invalidDate: true };
  }
  const year = Number(match[1]);
  const month = Number(match[2]);
  const day = Number(match[3]);
  if (year < 1900 || month < 1 || month > 12 || day < 1 || day > 31) {
    return { invalidDate: true };
  }
  const parsed = new Date(year, month - 1, day);
  if (
    parsed.getFullYear() !== year ||
    parsed.getMonth() !== month - 1 ||
    parsed.getDate() !== day
  ) {
    return { invalidDate: true };
  }
  return null;
}

export function formatearFechaLocalIso(fechaInput?: string | null): string | null {
  if (!fechaInput || typeof fechaInput !== 'string' || !fechaInput.trim()) {
    return null;
  }
  const fechaLimpia = fechaInput.trim().substring(0, 10);
  return `${fechaLimpia}T00:00:00`;
}

export function normalizarCantidadOpcional(valor: any): number | null {
  if (valor === null || valor === undefined || valor === '') {
    return null;
  }
  const num = Number(valor);
  return isNaN(num) ? null : num;
}

export function normalizarUnidadOpcional(valor: any): string | null {
  if (valor === null || valor === undefined) {
    return null;
  }
  const limpia = String(valor).trim();
  return limpia === '' ? null : limpia;
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
    const streamCreator = (query: any) =>
      this.pantryService.getList(query, { skipHandleError: true }).pipe(
        catchError((err) => {
          this.hasError = true;
          return throwError(() => err);
        })
      );

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

  obtenerFechaLocal(fecha?: string | null): Date | null {
    if (!fecha) {
      return null;
    }
    const soloFecha = fecha.substring(0, 10);
    const partes = soloFecha.split('-').map(Number);
    if (partes.length === 3 && !partes.some(isNaN)) {
      return new Date(partes[0], partes[1] - 1, partes[2]);
    }
    return new Date(fecha);
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
        const fechaFormateada = formatearFechaLocalIso(resultado.fechaven);
        const nuevoEstado = resultado.esConsumido ? 1 : 0;
        const datosActualizados: CreateUpdatePantryItemDto = {
          idPropietario: item.idPropietario!,
          nombreProducto: item.nombreProducto!,
          cantidad: normalizarCantidadOpcional(resultado.cantidad),
          unidad: normalizarUnidadOpcional(resultado.unidad),
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
      cantidad: [this.data?.cantidad ?? null, [Validators.min(0)]],
      unidad: [this.data?.unidad ?? null, [Validators.maxLength(32)]],
      fechaven: [fechaFormateada, [fechaValidaValidator]],
      esConsumido: [yaEstaConsumido]
    });
  }

  onFechaInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const control = this.miFormulario.get('fechaven');
    control?.markAsTouched();
    if (input?.validity?.badInput) {
      control?.setErrors({ invalidDate: true });
    } else if (control) {
      control.updateValueAndValidity();
    }
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
      const fechaFormateada = formatearFechaLocalIso(formVal.fechaven);
      const datosActualizados: CreateUpdatePantryItemDto = {
        idPropietario: item.idPropietario!,
        nombreProducto: item.nombreProducto!,
        cantidad: normalizarCantidadOpcional(formVal.cantidad),
        unidad: normalizarUnidadOpcional(formVal.unidad),
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
