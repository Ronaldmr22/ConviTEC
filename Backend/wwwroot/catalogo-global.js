const estado = document.querySelector("#estado");
const tabla = document.querySelector("#tabla-global");
const cuerpoTabla = document.querySelector("#registros-globales");

async function cargarCatalogoGlobal() {
    try {
        const respuesta = await fetch("/api/inventario-global");

        if (!respuesta.ok) {
            throw new Error(`HTTP ${respuesta.status}`);
        }

        const registros = await respuesta.json();

        if (registros.length === 0) {
            estado.textContent = "Esta sede todavía no tiene registros del catálogo global.";
            return;
        }

        for (const registro of registros) {
            const fila = cuerpoTabla.insertRow();
            const valores = [
                registro.codigoProducto,
                registro.codigoSede,
                registro.cantidad,
                registro.cantidadReservada,
                registro.cantidadDisponible
            ];

            for (const valor of valores) {
                fila.insertCell().textContent = valor;
            }
        }

        tabla.hidden = false;
        estado.textContent = `Registros de producto y sede: ${registros.length}`;
    } catch (error) {
        estado.textContent = "No se pudo cargar el catálogo global.";
        console.error(error);
    }
}

cargarCatalogoGlobal();
