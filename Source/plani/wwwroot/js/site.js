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

// =============================================================================
// CRUD TABLE HELPERS
// =============================================================================

/**
 * Toggle loading spinner and table visibility
 * @param {string} tableSelector - jQuery selector for the table (e.g., '#tablaAreas')
 * @param {boolean} show - true to show loading, false to show table
 */
function toggleLoading(tableSelector, show) {
	if (show) {
		$('#loading').removeClass('d-none');
		$(tableSelector).addClass('d-none');
	} else {
		$('#loading').addClass('d-none');
		$(tableSelector).removeClass('d-none');
	}
}

/**
 * Clear all add form errors
 * @param {string[]} fields - Array of field names (e.g., ['nombre', 'descripcion'])
 */
function clearAddErrors(fields) {
	fields.forEach(field => {
		$(`#add-${field}-error`).addClass('d-none').text('');
	});
}

/**
 * Show error on add form field
 * @param {string} field - Field name (e.g., 'nombre')
 * @param {string} message - Error message to display
 */
function showAddError(field, message) {
	$(`#add-${field}-error`).removeClass('d-none').text(message);
}

/**
 * Clear all edit form errors in a row
 * @param {jQuery} row - jQuery row element
 * @param {string[]} fields - Array of field names
 */
function clearEditErrors(row, fields) {
	fields.forEach(field => {
		row.find(`.edit-${field}-error`).addClass('d-none').text('');
	});
}

/**
 * Show error on edit form field in a row
 * @param {jQuery} row - jQuery row element
 * @param {string} field - Field name
 * @param {string} message - Error message
 */
function showEditError(row, field, message) {
	row.find(`.edit-${field}-error`).removeClass('d-none').text(message);
}

/**
 * Setup delete button click handler to show modal
 */
function setupDeleteButton() {
	$(document).on('click', '.btn-delete', function () {
		const id = $(this).data('id');
		const nombre = $(this).data('nombre');
		$('#nombreEliminar').text(nombre);
		$('#btnConfirmarEliminar').data('id', id);
		$('#ModalEliminar').modal('show');
	});
}

/**
 * Setup delete confirmation handler
 * @param {string} deleteUrl - URL for the delete action
 * @param {Function} onSuccess - Callback function(id) to execute on successful delete
 */
function setupDeleteConfirm(deleteUrl, onSuccess) {
	$('#btnConfirmarEliminar').on('click', function () {
		const id = $(this).data('id');
		const data = { id };

		$.ajax({
			url: deleteUrl,
			type: 'POST',
			contentType: 'application/json',
			data: JSON.stringify(data),
			success: function (response) {
				if (response.success) {
					$('#ModalEliminar').modal('hide');

					// Animate row removal
					const row = $(`tr[data-id="${id}"]`);
					row.fadeOut(300, function () {
						$(this).remove();
					});

					if (onSuccess) onSuccess(id);
					mostrarNotificacion(response.message, 'success');
				} else {
					if (response.errors && response.errors.length > 0) {
						mostrarNotificacion(response.errors.join(', '), 'danger');
					}
				}
			},
			error: function () {
				mostrarNotificacion('Error de conexión al eliminar', 'danger');
			}
		});
	});
}

/**
 * Initialize DataTable with Spanish language
 * @param {string} selector - jQuery selector for the table
 * @param {Object} options - Optional settings (order, lengthChange, etc.)
 */
function initDataTable(selector, options = {}) {
	const defaults = {
		order: [[0, "asc"]],
		language: { url: "/dataTables_es-ES.json" }
	};
	$(selector).DataTable({ ...defaults, ...options });
}

/**
 * Initialize Select2 with Bootstrap 4 theme
 * @param {string} selector - jQuery selector for the select element
 * @param {string} placeholder - Placeholder text
 */
function initSelect2(selector, placeholder = "Seleccione...") {
	setTimeout(function() {
		$(selector).select2({
			theme: 'bootstrap4',
			placeholder: placeholder,
			allowClear: true,
			width: '100%',
			minimumResultsForSearch: 0,
			dropdownAutoWidth: true,
			language: 'es'
		});
	}, 500);
}

/**
 * Fill a dropdown with options from an array of items
 * @param {string} selector - jQuery selector for the select element
 * @param {Array} items - Array of objects with id and nombre properties
 */
function fillDropdown(selector, items) {
	const select = $(selector);
	items.forEach(function(item) {
		select.append($('<option>', {
			value: item.id,
			text: item.nombre
		}));
	});
}