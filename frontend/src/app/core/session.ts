import { LoginResponse } from './models';

const STORAGE_KEY = 'padron.session';

export function readSession(): LoginResponse | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as LoginResponse;
    if (!parsed?.token || !parsed.username) {
      return null;
    }
    return parsed;
  } catch {
    return null;
  }
}

export function writeSession(session: LoginResponse): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
}

export function clearSession(): void {
  localStorage.removeItem(STORAGE_KEY);
}

export function isSessionActive(): boolean {
  const session = readSession();
  if (!session) {
    return false;
  }

  const expires = Date.parse(session.expiresAt);
  if (Number.isNaN(expires) || expires <= Date.now()) {
    clearSession();
    return false;
  }

  return true;
}
