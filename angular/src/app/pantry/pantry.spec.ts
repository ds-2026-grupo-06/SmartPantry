import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BehaviorSubject, NEVER, Observable, of, throwError } from 'rxjs';
import { ListService, LocalizationService, PagedResultDto } from '@abp/ng.core';
import { CoreTestingModule } from '@abp/ng.core/testing';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { PantryItemDto, PantryItemService } from '@proxy/pantry-items';
import { MiModalComponent, PantryComponent } from './pantry.component';

describe('PantryComponent & MiModalComponent (TP 10 - Consigna 8)', () => {
  let component: PantryComponent;
  let fixture: ComponentFixture<PantryComponent>;

  let isLoadingSubject: BehaviorSubject<boolean>;
  let getListResponse$: Observable<PagedResultDto<PantryItemDto>>;
  let getListCallCount: number;
  let updateCalls: Array<{ id: string; input: any }>;
  let deleteCalls: string[];
  let confirmationResult: Confirmation.Status;
  let warnCalls: Array<{ message: string; title: string }>;
  let listGetCallCount: number;

  let mockPantryService: Partial<PantryItemService>;
  let mockListService: Partial<ListService>;
  let mockConfirmationService: Partial<ConfirmationService>;
  let mockMatDialog: Partial<MatDialog>;

  beforeEach(async () => {
    isLoadingSubject = new BehaviorSubject<boolean>(false);
    getListResponse$ = of({ items: [], totalCount: 0 });
    getListCallCount = 0;
    updateCalls = [];
    deleteCalls = [];
    confirmationResult = Confirmation.Status.confirm;
    warnCalls = [];
    listGetCallCount = 0;

    mockPantryService = {
      getList: (() => {
        getListCallCount++;
        return getListResponse$;
      }) as any,
      update: ((id: string, input: any) => {
        updateCalls.push({ id, input });
        return of({ id, ...input } as PantryItemDto);
      }) as any,
      delete: ((id: string) => {
        deleteCalls.push(id);
        return of(undefined as void);
      }) as any,
    };

    mockListService = {
      isLoading$: isLoadingSubject.asObservable(),
      hookToQuery: ((streamCreator: (query: any) => Observable<any>) => {
        return streamCreator({ skipCount: 0, maxResultCount: 10 });
      }) as any,
      get: (() => {
        listGetCallCount++;
      }) as any,
    };

    mockConfirmationService = {
      warn: ((message: string, title: string) => {
        warnCalls.push({ message, title });
        return of(confirmationResult);
      }) as any,
    };

    mockMatDialog = {
      open: (() => ({
        afterClosed: () => of(null),
      })) as any,
    };

    await TestBed.configureTestingModule({
      imports: [
        NoopAnimationsModule,
        CoreTestingModule.withConfig(),
        PantryComponent,
        MiModalComponent,
      ],
      providers: [
        { provide: PantryItemService, useValue: mockPantryService },
        { provide: ConfirmationService, useValue: mockConfirmationService },
        { provide: MatDialog, useValue: mockMatDialog },
        { provide: MatDialogRef, useValue: { close: () => {} } },
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            item: {
              id: 'item-1',
              idPropietario: 'user-1',
              nombreProducto: 'Leche Entera',
              estado: 0,
            },
            cantidad: 2,
            unidad: 'litros',
            fechaven: '2026-12-20T00:00:00',
            estado: 0,
          },
        },
        {
          provide: LocalizationService,
          useValue: {
            instant: (key: string) => key,
            get: (key: string) => of(key),
          },
        },
      ],
    })
      .overrideComponent(PantryComponent, {
        set: {
          providers: [{ provide: ListService, useValue: mockListService }],
        },
      })
      .compileComponents();
  });

  function createPantryComponent(): void {
    fixture = TestBed.createComponent(PantryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('1. Estado de carga: muestra indicador de carga y oculta mensaje de lista vacía mientras la respuesta está pendiente', () => {
    isLoadingSubject.next(true);
    getListResponse$ = NEVER;

    createPantryComponent();

    const compiled = fixture.nativeElement as HTMLElement;
    const loadingAlerts = compiled.querySelectorAll('.alert-info');
    const emptyAlerts = compiled.querySelectorAll('.alert-warning');

    expect(loadingAlerts.length).toBeGreaterThan(0);
    expect(emptyAlerts.length).toBe(0);
  });

  it('2. Estado de error: activa hasError cuando falla el servicio y permite reintentar', () => {
    isLoadingSubject.next(false);
    getListResponse$ = throwError(() => new Error('BackEnd detenido'));

    createPantryComponent();

    const compiled = fixture.nativeElement as HTMLElement;
    const errorAlerts = compiled.querySelectorAll('.alert-danger');

    expect(component.hasError).toBe(true);
    expect(errorAlerts.length).toBeGreaterThan(0);

    component.reintentar();
    expect(component.hasError).toBe(false);
    expect(listGetCallCount).toBe(1);
  });

  it('3. Estado de lista vacía: muestra alerta de lista vacía cuando el usuario no tiene ítems y no está cargando', () => {
    isLoadingSubject.next(false);
    getListResponse$ = of({ items: [], totalCount: 0 });

    createPantryComponent();

    const compiled = fixture.nativeElement as HTMLElement;
    const emptyAlerts = compiled.querySelectorAll('.alert-warning');

    expect(component.hasError).toBe(false);
    expect(component.itemsDisponibles.length).toBe(0);
    expect(component.itemsConsumidos.length).toBe(0);
    expect(emptyAlerts.length).toBeGreaterThan(0);
  });

  it('4. Estado con resultados: muestra los ítems propios separando disponibles y consumidos, e informa datos faltantes (RF-09)', () => {
    isLoadingSubject.next(false);
    getListResponse$ = of({
      totalCount: 3,
      items: [
        {
          id: 'item-1',
          idPropietario: 'user-1',
          nombreProducto: 'Leche Entera',
          estado: 0,
          fechaVencimiento: '2026-12-20T00:00:00',
          cantidad: 2,
          unidad: 'litros',
        },
        {
          id: 'item-2',
          idPropietario: 'user-1',
          nombreProducto: 'Arroz Largo Fino',
          estado: 0,
          fechaVencimiento: null,
          cantidad: null,
          unidad: null,
        },
        {
          id: 'item-3',
          idPropietario: 'user-1',
          nombreProducto: 'Yogur Bebible',
          estado: 1,
          fechaVencimiento: '2026-11-25T00:00:00',
          cantidad: 1,
          unidad: 'botella',
        },
      ],
    });

    createPantryComponent();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(component.itemsDisponibles.length).toBe(2);
    expect(component.itemsConsumidos.length).toBe(1);
    expect(compiled.textContent).toContain('Leche Entera');
    expect(compiled.textContent).toContain('Arroz Largo Fino');
    expect(compiled.textContent).toContain('Yogur Bebible');
    expect(compiled.querySelectorAll('.fst-italic').length).toBeGreaterThan(0);
  });

  it('5. Edición con datos inválidos: rechaza cantidad negativa o fecha inválida y NO invoca al servidor', () => {
    const modalFixture = TestBed.createComponent(MiModalComponent);
    const modalComponent = modalFixture.componentInstance;
    modalFixture.detectChanges();

    // Cantidad negativa (-3) y fecha inválida ('')
    modalComponent.miFormulario.patchValue({
      cantidad: -3,
      unidad: 'litros',
      fechaven: '',
    });
    modalFixture.detectChanges();

    expect(modalComponent.miFormulario.invalid).toBe(true);
    expect(modalComponent.miFormulario.get('cantidad')?.invalid).toBe(true);
    expect(modalComponent.miFormulario.get('fechaven')?.invalid).toBe(true);

    modalComponent.guardar();

    expect(updateCalls.length).toBe(0);
  });

  it('6. Eliminación cancelada: pide confirmación previa y NO llama al servicio si el usuario cancela', () => {
    createPantryComponent();
    confirmationResult = Confirmation.Status.reject;

    const item: PantryItemDto = {
      id: 'item-99',
      nombreProducto: 'Harina',
      estado: 0,
    };

    component.eliminar(item);

    expect(warnCalls.length).toBe(1);
    expect(warnCalls[0].message).toBe('::Pantry:DeleteConfirmMessage');
    expect(warnCalls[0].title).toBe('::Pantry:DeleteConfirmTitle');
    expect(deleteCalls.length).toBe(0);
  });

  it('7. Eliminación confirmada: pide confirmación previa, invoca delete en el servicio y refresca la lista', () => {
    createPantryComponent();
    confirmationResult = Confirmation.Status.confirm;

    const item: PantryItemDto = {
      id: 'item-100',
      nombreProducto: 'Azúcar',
      estado: 0,
    };

    component.eliminar(item);

    expect(warnCalls.length).toBe(1);
    expect(deleteCalls).toEqual(['item-100']);
    expect(listGetCallCount).toBe(1);
  });
});
