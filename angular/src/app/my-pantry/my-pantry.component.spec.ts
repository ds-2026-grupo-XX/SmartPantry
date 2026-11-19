import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import {
  ListService,
  LocalizationService,
  PagedResultDto,
} from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import {
  EstadoItemDespensa,
  ItemDespensaService,
  type ItemDespensaDto,
} from '../proxy/index';
import { MyPantryComponent } from './my-pantry.component';

describe('MyPantryComponent', () => {
  const createSampleItems = (): ItemDespensaDto[] => [
    {
      id: 'item-1',
      productId: 'prod-1',
      nombreProducto: 'Leche entera',
      cantidad: 2,
      unidad: 'L',
      fechaVencimiento: '2026-12-10',
      estado: EstadoItemDespensa.Disponible,
    },
  ];

  async function setupComponent(getListFactory: () => any) {
    const itemDespensaServiceMock = {
      getList: vi.fn().mockImplementation(getListFactory),
      update: vi.fn(),
      markAsConsumed: vi.fn(),
      delete: vi.fn(),
    };

    const listServiceMock = {
      hookToQuery: vi.fn((streamCreator: (query: any) => any) =>
        streamCreator({ skipCount: 0, maxResultCount: 10, sorting: undefined })
      ),
      get: vi.fn(),
    };

    const confirmationServiceMock = {
      warn: vi.fn(),
    };

    const toasterServiceMock = {
      error: vi.fn(),
    };

    const localizationServiceMock = {
      instant: vi.fn((key: string) => key),
    };

    await TestBed.configureTestingModule({
      imports: [MyPantryComponent],
      providers: [
        provideRouter([]),
        { provide: ItemDespensaService, useValue: itemDespensaServiceMock },
        { provide: ListService, useValue: listServiceMock },
        { provide: ConfirmationService, useValue: confirmationServiceMock },
        { provide: ToasterService, useValue: toasterServiceMock },
        { provide: LocalizationService, useValue: localizationServiceMock },
      ],
    })
      .overrideComponent(MyPantryComponent, {
        set: {
          providers: [{ provide: ListService, useValue: listServiceMock }],
        },
      })
      .compileComponents();

    const fixture: ComponentFixture<MyPantryComponent> = TestBed.createComponent(MyPantryComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    return {
      fixture,
      component,
      itemDespensaServiceMock,
      listServiceMock,
      confirmationServiceMock,
      toasterServiceMock,
    };
  }

  beforeEach(() => {
    TestBed.resetTestingModule();
  });

  it('debe mostrar estado vacío cuando no hay ítems', async () => {
    const emptyResponse: PagedResultDto<ItemDespensaDto> = {
      items: [],
      totalCount: 0,
    };

    const { component, fixture } = await setupComponent(() => of(emptyResponse));

    expect(component.isLoading).toBe(false);
    expect(component.hasError).toBe(false);
    expect(fixture.nativeElement.textContent).toContain('::Pantry:Empty');
  });

  it('debe mostrar resultados cuando el servicio devuelve ítems', async () => {
    const pagedResponse: PagedResultDto<ItemDespensaDto> = {
      items: createSampleItems(),
      totalCount: 1,
    };

    const { component, fixture } = await setupComponent(() => of(pagedResponse));

    expect(component.isLoading).toBe(false);
    expect(component.hasError).toBe(false);
    expect(fixture.nativeElement.textContent).toContain('Leche entera');
  });

  it('debe mostrar estado de error cuando falla getList', async () => {
    const { component, fixture } = await setupComponent(() =>
      throwError(() => new Error('Network error'))
    );

    expect(component.isLoading).toBe(false);
    expect(component.hasError).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('::Pantry:LoadError');
  });

  it('no debe llamar a update si la cantidad es negativa o la fecha es inválida', async () => {
    const items = createSampleItems();
    const { component, itemDespensaServiceMock } = await setupComponent(() =>
      of({
        items,
        totalCount: 1,
      })
    );

    component.startEdit(items[0]);
    component.editForm.patchValue({
      cantidad: -3,
      fechaVencimiento: 'fecha-invalida',
    });
    component.saveEdit();

    expect(component.editForm.invalid).toBe(true);
    expect(itemDespensaServiceMock.update).not.toHaveBeenCalled();
  });

  it('no debe llamar a delete si el usuario cancela la confirmación', async () => {
    const items = createSampleItems();
    const { component, itemDespensaServiceMock, confirmationServiceMock } = await setupComponent(
      () =>
        of({
          items,
          totalCount: 1,
        })
    );

    confirmationServiceMock.warn.mockReturnValue(of(Confirmation.Status.reject));
    component.deleteItem(items[0]);

    expect(confirmationServiceMock.warn).toHaveBeenCalled();
    expect(itemDespensaServiceMock.delete).not.toHaveBeenCalled();
  });

  it('debe llamar a delete si el usuario confirma la eliminación', async () => {
    const items = createSampleItems();
    const { component, itemDespensaServiceMock, confirmationServiceMock } = await setupComponent(
      () =>
        of({
          items,
          totalCount: 1,
        })
    );

    confirmationServiceMock.warn.mockReturnValue(of(Confirmation.Status.confirm));
    itemDespensaServiceMock.delete.mockReturnValue(of(void 0));

    component.deleteItem(items[0]);

    expect(itemDespensaServiceMock.delete).toHaveBeenCalledWith('item-1');
  });
});
