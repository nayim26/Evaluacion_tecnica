import { HttpErrorResponse } from '@angular/common/http';

interface ApiErrorBody {
  message?: string;
  title?: string;
  errors?: unknown;
}

function collectErrors(errors: unknown): string {
  if (Array.isArray(errors)) {
    return errors.filter((item): item is string => typeof item === 'string').join(' ');
  }

  if (errors && typeof errors === 'object') {
    return Object.values(errors)
      .flatMap((value) => (Array.isArray(value) ? value : [value]))
      .filter((item): item is string => typeof item === 'string')
      .join(' ');
  }

  return '';
}

export function readApiError(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  if (error.status === 0) {
    return 'No se pudo conectar con la API. Compruebe que el backend esté en ejecución.';
  }

  const body = error.error as ApiErrorBody | string | null;
  if (typeof body === 'string' && body.trim()) {
    return body;
  }

  if (body && typeof body === 'object') {
    const details = collectErrors(body.errors);
    if (details) {
      return details;
    }

    const message = typeof body.message === 'string' ? body.message : '';
    if (message) {
      return message;
    }

    if (typeof body.title === 'string' && body.title.trim()) {
      return body.title;
    }
  }

  return fallback;
}
