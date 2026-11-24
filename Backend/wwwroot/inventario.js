const estado = document.querySelector("#estado");
const tabla = document.querySelector("#tabla-inventario");
const cuerpoTabla = document.querySelector("#productos");

const formatoPrecio = new Intl.NumberFormat("es-CR", {
    style: "currency",
    currency: "CRC"
});

async function cargarInventario() {
    try {
        const respuesta = await fetch("/api/inventario");

        if (!respuesta.ok) {
            throw new Error(`HTTP ${respuesta.status}`);
        }

        const productos = await respuesta.json();

        if (productos.length === 0) {
            estado.textContent = "La sede no tiene productos registrados.";
            return;
        }

        for (const producto of productos) {
            const fila = cuerpoTabla.insertRow();
            const disponible = producto.cantidad - producto.cantidadReservada;
            const valores = [
                producto.codigo,
                producto.nombre,
                producto.categoria,
                formatoPrecio.format(producto.precio),
                producto.cantidad,
                disponible
            ];

            for (const valor of valores) {
                fila.insertCell().textContent = valor;
            }
        }

        tabla.hidden = false;
        estado.textContent = `Productos registrados: ${productos.length}`;
    } catch (error) {
        estado.textContent = "No se pudo cargar el inventario de la sede.";
        console.error(error);
    }
}

cargarInventario();
