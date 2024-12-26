window.addEventListener('DOMContentLoaded', function () {
	const formShortUrl = this.document.getElementById('form_short_url')
	const urlElement = this.document.getElementById('url')
	const btn_result__copy = this.document.getElementById('result__copy')
	 
	$(formShortUrl).validate({
		errorPlacement: function ($error, $element) {
			var id = $element.attr("id");
			console.log($error, id)

			$(`#error_${id}`).append($error);
		},
		rules: {
			url: {
				required: true,
				url: true
			}
		},
		messages: {
			url: {
				required: "La URL es requerida",
				url: "Ingrese una URL valida"
			}
		},
	})

	formShortUrl.addEventListener('submit', function (evt) {
		evt.preventDefault();

		const formularioValido = $(formShortUrl).valid();
		if (!formularioValido) return;

		fetch('/Home/Short', {
			method: 'POST',
			body: JSON.stringify({ LongURL: urlElement.value }),
			headers: {
				'Content-Type': 'application/json'
			}
		})
			.then(async (response)=> {
				const data = await response.json();
		 
				if (!response.ok) {
					throw new Error(data[0].messages[0])
				}

				return data 
			})
			.then(shortURL => {
				const result__url = document.getElementById('result__url')

				result__url.parentElement.classList.remove('d-none')

				const resultURL = `${window.location.origin}/${shortURL.shortUniqueId}`

				result__url.innerHTML = `
					<a class="btn btn-link" target="_blank" href="${resultURL}"> 
						${resultURL}

						<svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="align-top"><path stroke="none" d="M0 0h24v24H0z" fill="none" /><path d="M12 6h-6a2 2 0 0 0 -2 2v10a2 2 0 0 0 2 2h10a2 2 0 0 0 2 -2v-6" /><path d="M11 13l9 -9" /><path d="M15 4h5v5" /></svg>
					</a>
				`

			})
			.catch(error => {

				$('#error_url').html(error.message)
			})
		 
	})


	if (btn_result__copy) {
		btn_result__copy.addEventListener('click', function () {
			const short = document.querySelector('#result__url a')


			var tooltip = new bootstrap.Tooltip(btn_result__copy, {
				trigger: 'click',
				title: 'Copiado!',
				delay: { "show": 0, "hide": 500 }
			})

			navigator.clipboard
				.writeText(short.href)
				.then(() => {
					tooltip.show()

					setTimeout(() => {
						tooltip.hide()
					}, 800)
				})
				.catch(error => {
					console.log(error)
				})
		})
	}
})