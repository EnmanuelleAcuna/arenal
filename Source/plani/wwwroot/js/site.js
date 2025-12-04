// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

/**
 * Función global para mostrar notificaciones Toast usando Bootstrap 4
 * @param {string} mensaje - El mensaje a mostrar en el toast
 * @param {string} tipo - El tipo de toast: 'success', 'danger', 'warning', 'info' (default: 'success')
 */
function mostrarToast(mensaje, tipo = 'success') {
	var iconos = {
		success: 'fa-check-circle',
		danger: 'fa-exclamation-circle',
		warning: 'fa-exclamation-triangle',
		info: 'fa-info-circle'
	};

	var colores = {
		success: 'bg-success',
		danger: 'bg-danger',
		warning: 'bg-warning',
		info: 'bg-info'
	};

	var titulos = {
		success: 'Éxito',
		danger: 'Error',
		warning: 'Advertencia',
		info: 'Información'
	};

	var toastHtml = `
		<div class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-delay="4000">
			<div class="toast-header ${colores[tipo]} text-white">
				<i class="fas ${iconos[tipo]} mr-2"></i>
				<strong class="mr-auto">${titulos[tipo]}</strong>
				<button type="button" class="ml-2 mb-1 close text-white" data-dismiss="toast" aria-label="Close">
					<span aria-hidden="true">&times;</span>
				</button>
			</div>
			<div class="toast-body">
				${mensaje}
			</div>
		</div>
	`;

	var $toast = $(toastHtml);
	$('#toastContainer').append($toast);
	$toast.toast('show');

	$toast.on('hidden.bs.toast', function () {
		$(this).remove();
	});
}

/**
* Función para mostrar notificaciones desde TempData
* Llama a mostrarToast si el mensaje no es nulo/vacío
* @param {string} mensaje - El mensaje desde TempData["ToastMessage"]
* @param {string} tipo - El tipo desde TempData["ToastType"]
*/
function mostrarNotificacion(mensaje, tipo) {
	if (mensaje && mensaje.trim() !== '') {
		mostrarToast(mensaje, tipo || 'success');
	}
}