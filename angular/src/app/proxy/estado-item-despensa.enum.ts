import { mapEnumToOptions } from '@abp/ng.core';

export enum EstadoItemDespensa {
  Disponible = 1,
  Consumido = 2,
}

export const estadoItemDespensaOptions = mapEnumToOptions(EstadoItemDespensa);
