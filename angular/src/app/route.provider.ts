import { RoutesService, eLayoutType } from '@abp/ng.core';
import { APP_INITIALIZER } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  { provide: APP_INITIALIZER, useFactory: configureRoutes, deps: [RoutesService], multi: true },
];

function configureRoutes(routesService: RoutesService) {
  return () => {
    routesService.add([
      {
        path: '/pantry',
        name: '::Menu:MyPantry', // Clave de localización
        iconClass: 'fas fa-box',
        order: 2,
        layout: eLayoutType.application,
      },
    ]);
  };
}
