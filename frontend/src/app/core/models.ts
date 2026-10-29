export interface Usuario {
  id: number;
  username: string;
  email: string;
  estado: boolean;
  fechaCreacion: string;
}

export interface LoginResponse {
  token: string;
  id: number;
  username: string;
  email: string;
  expiresAt: string;
}

export interface UsuarioCreate {
  username: string;
  email: string;
  password: string;
  estado: boolean;
}

export interface UsuarioUpdate {
  username: string;
  email: string;
  password?: string;
  estado: boolean;
}
