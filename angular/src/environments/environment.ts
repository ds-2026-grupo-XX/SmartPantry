import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'SmartPantry',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44323/',
    redirectUri: baseUrl,
    clientId: 'SmartPantry_App',
    responseType: 'code',
    scope: 'offline_access SmartPantry',
    requireHttps: true,
  },
  apis: {
    default: {
      url: 'https://localhost:44323',
      rootNamespace: 'SmartPantry',
    },
  },
} as Environment;
