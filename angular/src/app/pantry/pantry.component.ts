import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreModule, ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared'; // <-- 1. Importamos el servicio de confirmación
import { PantryItemService, PantryItemDto, CreateUpdatePantryItemDto } from '@proxy/pantry-items';
import { MatDialog } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

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
    private confirmation: ConfirmationService // <-- 2. Inyectamos el ConfirmationService
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

  // 3. Método de eliminación con confirmación previa de ABP
  eliminar(item: PantryItemDto): void {
    this.confirmation.warn('¿Estás seguro de eliminar este ítem?', 'Confirmar eliminación').subscribe((status) => {
      // Si el usuario hace clic en "Confirmar" (status.valueOf() === Confirmation.Status.confirm)
      if (status === Confirmation.Status.confirm && item.id) {
        this.pantryService.delete(item.id).subscribe(() => {
          this.list.get(); // Refresca las listas tras borrar
        });
      }
    });
  }

  abrirModal(item: PantryItemDto): void {
    const dialogRef = this.dialog.open(MiModalComponent, {
      width: '400px',
      data: { cantidad: item.cantidad, unidad: item.unidad, fechaven: item.fechaVencimiento, estado: item.estado }
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado && item.id) {
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

        this.pantryService.update(item.id!, datosActualizados).subscribe(() => {
          this.list.get();
        });
      }
    });
  }
}

@Component({
  selector: 'app-mi-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatInputModule, MatButtonModule],
  templateUrl: './editpantry.component.html'
})
export class MiModalComponent implements OnInit {
  miFormulario!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<MiModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  ngOnInit(): void {
    let fechaFormateada = '';
    if (this.data?.fechaven) {
      fechaFormateada = this.data.fechaven.substring(0, 10);
    }
    const yaEstaConsumido = this.data?.estado === 1;
    this.miFormulario = this.fb.group({
      cantidad: [this.data?.cantidad || '', [Validators.required, Validators.min(1)]],
      unidad: [this.data?.unidad || '', [Validators.required, Validators.minLength(1)]],
      fechaven: [fechaFormateada, [Validators.required]],
      esConsumido: [yaEstaConsumido]
    });
  }

  guardar(): void {
    if (this.miFormulario.valid) {
      this.dialogRef.close(this.miFormulario.value);
    } else {
      this.miFormulario.markAllAsTouched();
    }
  }

  cancelar(): void {
    this.dialogRef.close();
  }
}
