import type { AuditedEntityDto } from '@abp/ng.core';
import type { EstadoItemDespensa } from './estado-item-despensa.enum';

export interface BarcodeSearchInputDto {
  codigoDeBarras: string;
}

export interface CreateUpdateProductDto {
  codigoDeBarras: string;
  nombreVisible: string;
  imagen?: string | null;
  nutriScore?: number | null;
  nova?: number | null;
  marca?: string | null;
}

export interface ExternalProductDto {
  encontrado?: boolean;
  codigoDeBarras?: string;
  nombreVisible?: string | null;
  imagen?: string | null;
  nutriScore?: number | null;
  nova?: number | null;
  marca?: string | null;
}

export interface ItemDespensaDto extends AuditedEntityDto<string> {
  productId?: string;
  nombreProducto?: string;
  cantidad?: number;
  unidad?: string | null;
  fechaVencimiento?: string | null;
  estado?: EstadoItemDespensa;
}

export interface ProductDto extends AuditedEntityDto<string> {
  codigoDeBarras?: string;
  nombreVisible?: string;
  imagen?: string | null;
  nutriScore?: number | null;
  nova?: number | null;
  marca?: string | null;
}

export interface UpdateItemDespensaDto {
  cantidad?: number;
  unidad?: string | null;
  fechaVencimiento?: string | null;
}
