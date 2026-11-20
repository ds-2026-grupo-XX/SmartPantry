import { EMPTY, catchError } from 'rxjs';
import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import {
  ListService,
  LocalizationPipe,
  PagedResultDto,
} from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import {
  EstadoItemDespensa,
  ItemDespensaService,
  type ItemDespensaDto,
  type UpdateItemDespensaDto,
} from '../proxy/index';

const MAX_UNIT_LENGTH = 50;

function validDateValidator(isBadInputFn: () => boolean): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (isBadInputFn()) {
      return { invalidDate: true };
    }

    const value = control.value;
    if (!value) {
      return null;
    }

    const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(String(value).trim());
    if (!match) {
      return { invalidDate: true };
    }

    const year = Number(match[1]);
    const month = Number(match[2]);
    const day = Number(match[3]);
    const date = new Date(year, month - 1, day);

    if (
      date.getFullYear() !== year ||
      date.getMonth() !== month - 1 ||
      date.getDate() !== day
    ) {
      return { invalidDate: true };
    }

    return null;
  };
}

@Component({
  selector: 'app-my-pantry',
  standalone: true,
  imports: [CommonModule, DatePipe, LocalizationPipe, ReactiveFormsModule],
  providers: [ListService],
  template: `
    <div class="container py-4">
      <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
        <h2 class="mb-0">{{ '::Menu:MyPantry' | abpLocalization }}</h2>
      </div>

      <!-- Formulario de edición -->
      @if (editingItem) {
        <div class="card mb-4 border-primary">
          <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
            <span>{{ '::Pantry:EditTitle' | abpLocalization }}: <strong>{{ editingItem.nombreProducto }}</strong></span>
            <button type="button" class="btn-close btn-close-white" aria-label="Close" (click)="cancelEdit()"></button>
          </div>
          <div class="card-body">
            <form [formGroup]="editForm" (ngSubmit)="saveEdit()">
              <div class="row g-3">
                <div class="col-12 col-md-4">
                  <label class="form-label" for="editCantidad">{{ '::Pantry:Quantity' | abpLocalization }}</label>
                  <input
                    id="editCantidad"
                    type="number"
                    min="0"
                    class="form-control"
                    formControlName="cantidad"
                  />
                  @if (editForm.get('cantidad')?.invalid && (editForm.get('cantidad')?.touched || editForm.get('cantidad')?.dirty)) {
                    <div class="text-danger small mt-1">
                      {{ '::Pantry:InvalidQuantity' | abpLocalization }}
                    </div>
                  }
                </div>

                <div class="col-12 col-md-4">
                  <label class="form-label" for="editUnidad">{{ '::Pantry:Unit' | abpLocalization }}</label>
                  <input
                    id="editUnidad"
                    type="text"
                    class="form-control"
                    formControlName="unidad"
                  />
                  @if (editForm.get('unidad')?.invalid && (editForm.get('unidad')?.touched || editForm.get('unidad')?.dirty)) {
                    <div class="text-danger small mt-1">
                      {{ '::Pantry:InvalidUnit' | abpLocalization }}
                    </div>
                  }
                </div>

                <div class="col-12 col-md-4">
                  <label class="form-label" for="editFechaVencimiento">{{ '::Pantry:ExpirationDate' | abpLocalization }}</label>
                  <input
                    id="editFechaVencimiento"
                    type="date"
                    class="form-control"
                    formControlName="fechaVencimiento"
                    (input)="onDateInput($event)"
                    (blur)="onDateInput($event)"
                  />
                  @if (editForm.get('fechaVencimiento')?.invalid && (editForm.get('fechaVencimiento')?.touched || editForm.get('fechaVencimiento')?.dirty || isDateInputInvalid)) {
                    <div class="text-danger small mt-1">
                      {{ '::Pantry:InvalidExpirationDate' | abpLocalization }}
                    </div>
                  }
                </div>
              </div>

              <div class="d-flex justify-content-end gap-2 mt-3">
                <button type="button" class="btn btn-secondary" (click)="cancelEdit()" [disabled]="isSaving">
                  {{ '::Pantry:Cancel' | abpLocalization }}
                </button>
                <button type="submit" class="btn btn-primary" [disabled]="editForm.invalid || isSaving">
                  {{ '::Pantry:Save' | abpLocalization }}
                </button>
              </div>
            </form>
          </div>
        </div>
      }

      <!-- 1. Estado de carga -->
      @if (isLoading) {
        <div class="alert alert-info" role="status">
          {{ '::Pantry:Loading' | abpLocalization }}
        </div>
      }

      <!-- 2. Estado de error -->
      @else if (hasError) {
        <div class="alert alert-danger d-flex flex-wrap justify-content-between align-items-center gap-2" role="alert">
          <span>{{ '::Pantry:LoadError' | abpLocalization }}</span>
          <button type="button" class="btn btn-sm btn-outline-danger" (click)="loadItems()">
            {{ '::Pantry:Retry' | abpLocalization }}
          </button>
        </div>
      }

      <!-- 3. Estado de lista vacía -->
      @else if ((items.items?.length ?? 0) === 0) {
        <div class="alert alert-secondary">
          {{ '::Pantry:Empty' | abpLocalization }}
        </div>
      }

      <!-- 4. Estado con resultados -->
      @else {
        <div class="table-responsive">
          <table class="table table-striped align-middle">
            <thead>
              <tr>
                <th>{{ '::Pantry:Product' | abpLocalization }}</th>
                <th>{{ '::Pantry:Quantity' | abpLocalization }}</th>
                <th>{{ '::Pantry:Unit' | abpLocalization }}</th>
                <th>{{ '::Pantry:ExpirationDate' | abpLocalization }}</th>
                <th>{{ '::Pantry:Status' | abpLocalization }}</th>
                <th class="text-end">{{ '::Pantry:Actions' | abpLocalization }}</th>
              </tr>
            </thead>
            <tbody>
              @for (item of items.items ?? []; track item.id) {
                <tr>
                  <td>
                    @if (item.nombreProducto) {
                      {{ item.nombreProducto }}
                    } @else {
                      <span class="text-muted">{{ '::Pantry:NotAvailable' | abpLocalization }}</span>
                    }
                  </td>
                  <td>
                    @if (item.cantidad !== undefined && item.cantidad !== null) {
                      {{ item.cantidad }}
                    } @else {
                      <span class="text-muted">{{ '::Pantry:NotAvailable' | abpLocalization }}</span>
                    }
                  </td>
                  <td>
                    @if (item.unidad) {
                      {{ item.unidad }}
                    } @else {
                      <span class="text-muted">{{ '::Pantry:NotAvailable' | abpLocalization }}</span>
                    }
                  </td>
                  <td>
                    @if (item.fechaVencimiento) {
                      {{ item.fechaVencimiento | date: 'shortDate' }}
                    } @else {
                      <span class="text-muted">{{ '::Pantry:NotAvailable' | abpLocalization }}</span>
                    }
                  </td>
                  <td>
                    @if (item.estado === EstadoItemDespensa.Consumido) {
                      <span class="badge bg-secondary">{{ '::Pantry:StatusConsumed' | abpLocalization }}</span>
                    } @else {
                      <span class="badge bg-success">{{ '::Pantry:StatusAvailable' | abpLocalization }}</span>
                    }
                  </td>
                  <td class="text-end">
                    <div class="d-inline-flex flex-wrap justify-content-end gap-1">
                      <button
                        type="button"
                        class="btn btn-sm btn-outline-primary"
                        (click)="startEdit(item)"
                        [disabled]="busyItemId === item.id"
                      >
                        {{ '::Pantry:Edit' | abpLocalization }}
                      </button>

                      @if (item.estado !== EstadoItemDespensa.Consumido) {
                        <button
                          type="button"
                          class="btn btn-sm btn-outline-success"
                          (click)="markAsConsumed(item)"
                          [disabled]="busyItemId === item.id"
                        >
                          {{ '::Pantry:MarkAsConsumed' | abpLocalization }}
                        </button>
                      }

                      <button
                        type="button"
                        class="btn btn-sm btn-outline-danger"
                        (click)="deleteItem(item)"
                        [disabled]="busyItemId === item.id"
                      >
                        {{ '::Pantry:Delete' | abpLocalization }}
                      </button>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `,
})
export class MyPantryComponent implements OnInit {
  private readonly itemDespensaService = inject(ItemDespensaService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  public readonly list = inject(ListService);

  protected readonly EstadoItemDespensa = EstadoItemDespensa;

  items: PagedResultDto<ItemDespensaDto> = {
    items: [],
    totalCount: 0,
  };

  isLoading = false;
  hasError = false;
  isSaving = false;
  isDateInputInvalid = false;
  busyItemId: string | null = null;
  editingItem: ItemDespensaDto | null = null;

  editForm: FormGroup = this.fb.group({
    cantidad: [0, [Validators.required, Validators.min(0)]],
    unidad: ['', [Validators.maxLength(MAX_UNIT_LENGTH)]],
    fechaVencimiento: [null, [validDateValidator(() => this.isDateInputInvalid)]],
  });

  ngOnInit(): void {
    this.loadItems();
  }

  onDateInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.isDateInputInvalid = input.validity?.badInput ?? false;
    const control = this.editForm.get('fechaVencimiento');
    control?.markAsDirty();
    control?.updateValueAndValidity();
  }

  loadItems(): void {
    this.isLoading = true;
    this.hasError = false;

    const streamCreator = (query: any) =>
      this.itemDespensaService
        .getList(
          {
            skipCount: query.skipCount ?? 0,
            maxResultCount: query.maxResultCount ?? 10,
            sorting: query.sorting,
          },
          { skipHandleError: true }
        )
        .pipe(
          catchError(() => {
            this.isLoading = false;
            this.hasError = true;
            this.cdr.detectChanges();
            return EMPTY;
          })
        );

    this.list.hookToQuery(streamCreator).subscribe({
      next: (response) => {
        this.items = response;
        this.isLoading = false;
        this.hasError = false;
        this.cdr.detectChanges();
      },
    });
  }

  startEdit(item: ItemDespensaDto): void {
    this.editingItem = item;
    this.isDateInputInvalid = false;
    this.editForm.reset({
      cantidad: item.cantidad ?? 0,
      unidad: item.unidad ?? '',
      fechaVencimiento: item.fechaVencimiento ?? null,
    });
    this.cdr.detectChanges();
  }

  cancelEdit(): void {
    this.editingItem = null;
    this.isSaving = false;
    this.isDateInputInvalid = false;
    this.cdr.detectChanges();
  }

  saveEdit(): void {
    if (!this.editingItem?.id || this.editForm.invalid || this.isSaving) {
      return;
    }

    this.isSaving = true;
    const raw = this.editForm.value;
    const input: UpdateItemDespensaDto = {
      cantidad: Number(raw.cantidad),
      unidad: raw.unidad?.trim() ? raw.unidad.trim() : undefined,
      fechaVencimiento: raw.fechaVencimiento ? raw.fechaVencimiento : undefined,
    };

    this.itemDespensaService.update(this.editingItem.id, input).subscribe({
      next: () => {
        this.isSaving = false;
        this.editingItem = null;
        this.isDateInputInvalid = false;
        this.list.get();
        this.cdr.detectChanges();
      },
      error: () => {
        this.isSaving = false;
        this.toaster.error('::Pantry:ActionError');
        this.cdr.detectChanges();
      },
    });
  }

  markAsConsumed(item: ItemDespensaDto): void {
    if (!item.id || this.busyItemId) {
      return;
    }

    this.busyItemId = item.id;
    this.itemDespensaService.markAsConsumed(item.id).subscribe({
      next: () => {
        this.busyItemId = null;
        this.list.get();
        this.cdr.detectChanges();
      },
      error: () => {
        this.busyItemId = null;
        this.toaster.error('::Pantry:ActionError');
        this.cdr.detectChanges();
      },
    });
  }

  deleteItem(item: ItemDespensaDto): void {
    if (!item.id || this.busyItemId) {
      return;
    }

    this.confirmation
      .warn('::Pantry:DeleteConfirmMessage', '::Pantry:DeleteConfirmTitle')
      .subscribe((status) => {
        if (status !== Confirmation.Status.confirm || !item.id) {
          return;
        }

        this.busyItemId = item.id;
        this.itemDespensaService.delete(item.id).subscribe({
          next: () => {
            if (this.editingItem?.id === item.id) {
              this.editingItem = null;
            }
            this.busyItemId = null;
            this.list.get();
            this.cdr.detectChanges();
          },
          error: () => {
            this.busyItemId = null;
            this.toaster.error('::Pantry:ActionError');
            this.cdr.detectChanges();
          },
        });
      });
  }
}
