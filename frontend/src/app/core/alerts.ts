import Swal from 'sweetalert2';

const confirmColor = '#227f96';

export function alertSuccess(title: string) {
  return Swal.fire({
    icon: 'success',
    title,
    confirmButtonText: 'Aceptar',
    confirmButtonColor: confirmColor,
  });
}

export function alertError(text: string) {
  return Swal.fire({
    icon: 'error',
    title: 'No se pudo completar',
    text,
    confirmButtonText: 'Aceptar',
    confirmButtonColor: confirmColor,
  });
}

export function alertRequirements(messages: string[]) {
  const items = messages.map((message) => `<li>${escapeHtml(message)}</li>`).join('');
  return Swal.fire({
    icon: 'warning',
    title: 'Revise los datos',
    html: `<ul style="margin:0;padding-left:1.15rem;text-align:left">${items}</ul>`,
    confirmButtonText: 'Aceptar',
    confirmButtonColor: confirmColor,
  });
}

export function confirmRemoval(username: string) {
  return Swal.fire({
    icon: 'warning',
    title: 'Eliminar usuario',
    text: `Se eliminará a ${username}. Esta acción no se puede deshacer.`,
    showCancelButton: true,
    confirmButtonText: 'Eliminar',
    cancelButtonText: 'Cancelar',
    confirmButtonColor: '#c0352b',
    cancelButtonColor: '#8d9ea6',
    reverseButtons: true,
  });
}

function escapeHtml(value: string): string {
  return value.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');
}
