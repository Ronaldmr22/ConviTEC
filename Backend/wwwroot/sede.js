async function mostrarSede() {
    const etiqueta = document.querySelector("#sede-actual");

    try {
        const respuesta = await fetch("/api/sede");

        if (!respuesta.ok) {
            throw new Error(`GET /api/sede devolvió HTTP ${respuesta.status}`);
        }

        const sede = await respuesta.json();
        etiqueta.textContent = `Sede ${sede.nombre}`;
    } catch (error) {
        etiqueta.textContent = "Sede sin identificar";
        console.error(error);
    }
}

mostrarSede();
