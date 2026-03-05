import type { JsonFormsRendererRegistryEntry } from '@jsonforms/core';
import { EnergyRatingRenderer, energyRatingTester } from './EnergyRatingRenderer';
import { OrientationRenderer, orientationTester } from './OrientationRenderer';
import { PropertyTypeRenderer, propertyTypeTester } from './PropertyTypeRenderer';
import { OperationTypeRenderer, operationTypeTester } from './OperationTypeRenderer';

export const propertyRenderers: JsonFormsRendererRegistryEntry[] = [
  { tester: energyRatingTester, renderer: EnergyRatingRenderer },
  { tester: orientationTester, renderer: OrientationRenderer },
  { tester: propertyTypeTester, renderer: PropertyTypeRenderer },
  { tester: operationTypeTester, renderer: OperationTypeRenderer },
];
