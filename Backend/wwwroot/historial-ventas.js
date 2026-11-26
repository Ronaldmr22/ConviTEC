const estado = document.querySelector("#estado-ventas");
const tabla = document.querySelector("#tabla-ventas");
const filas = document.querySelector("#filas-ventas");

const moneda = new Intl.NumberFormat("es-CR", {
    style: "currency",
    currency: "CRC"
});

const fechaEntrega = new Intl.DateTimeFormat("es-CR", {
    dateStyle: "short",
    timeStyle: "short",
    timeZone: "America/Costa_Rica"
});

async function cargarVentas() {
    try {
        const respuesta = await fetch("/api/ventas");

        if (!respuesta.ok) {
            throw new Error(`GET /api/ventas devolvió HTTP ${respuesta.status}`);
        }

        const ventas = await respuesta.json();

        if (ventas.length === 0) {
            estado.textContent = "Esta sede todavía no tiene ventas registradas.";
            return;
        }

        for (const venta of ventas) {
            const cantidadFilas = venta.productos.length;

            for (let indice = 0; indice < cantidadFilas; indice++) {
                const producto = venta.productos[indice];
                const fila = filas.insertRow();

                // El pedido y la fecha abarcan todas las filas de esta venta.
                if (indice === 0) {
                    const pedido = fila.insertCell();
                    pedido.textContent = venta.idPedido;
                    pedido.rowSpan = cantidadFilas;

                    const fecha = fila.insertCell();
                    fecha.textContent = fechaEntrega.format(new Date(venta.fechaEntrega));
                    fecha.rowSpan = cantidadFilas;
                }

                fila.insertCell().textContent = producto.nombreProducto;
                fila.insertCell().textContent = producto.cantidad;
                fila.insertCell().textContent = moneda.format(producto.precioUnitario);

                // Muestra el total una sola vez por pedido.
                if (indice === 0) {
                    const total = fila.insertCell();
                    total.textContent = moneda.format(venta.montoTotal);
                    total.rowSpan = cantidadFilas;
                }
            }
        }

        tabla.hidden = false;
        estado.textContent = `Ventas registradas: ${ventas.length}`;
    } catch (error) {
        estado.textContent = "No se pudo cargar el historial de ventas.";
        console.error(error);
    }
}

cargarVentas();
