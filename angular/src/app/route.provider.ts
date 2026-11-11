import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';
export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];
function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/books',
        name: '::Menu:Books',
        iconClass: 'fas fa-book',
        layout: eLayoutType.application,
        requiredPolicy: 'SmartPantry.Books',
      },
      {
        path: '/authors',
        name: '::Menu:Authors',
        iconClass: 'fas fa-user-edit',
        layout: eLayoutType.application,
        requiredPolicy: 'SmartPantry.Authors',
      },
      {
        path: '/pantry',
        name: '::Menu:MyPantry', // Clave que se usará para la traducción
        iconClass: 'fas fa-box', // Ícono de FontAwesome
        order: 2,
        layout: eLayoutType.application,
      },
  ]);
}
