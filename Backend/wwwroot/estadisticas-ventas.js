const estado = document.querySelector("#estado-estadisticas");
const resumen = document.querySelector("#resumen-ventas");
const ingresos = document.querySelector("#ingresos-totales");
const estrella = document.querySelector("#producto-estrella");
const tabla = document.querySelector("#tabla-rendimiento");
const filas = document.querySelector("#filas-rendimiento");
const sinRendimiento = document.querySelector("#sin-rendimiento");

const moneda = new Intl.NumberFormat("es-CR", {
    style: "currency",
    currency: "CRC",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
});

async function cargarEstadisticas() {
    try {
        const respuesta = await fetch("/api/ventas/estadisticas");

        if (!respuesta.ok) {
            throw new Error(`GET /api/ventas/estadisticas devolvió HTTP ${respuesta.status}`);
        }

        const datos = await respuesta.json();
        ingresos.textContent = moneda.format(datos.ingresosTotales);

        const producto = datos.productoEstrella;
        estrella.textContent = producto
            ? `${producto.nombreProducto} · ${moneda.format(producto.ingresos)}`
            : "Esta sede todavía no tiene ventas registradas.";

        // La API en C# ya devuelve los pedidos ordenados por rendimiento.
        for (const pedido of datos.pedidosPorRendimiento) {
            const fila = filas.insertRow();
            fila.insertCell().textContent = pedido.idPedido;
            fila.insertCell().textContent = moneda.format(pedido.montoTotal);
            fila.insertCell().textContent = pedido.minutosTraslado;
            fila.insertCell().textContent = moneda.format(pedido.colonesPorMinuto);
        }

        const hayRendimientos = datos.pedidosPorRendimiento.length > 0;
        tabla.hidden = !hayRendimientos;
        sinRendimiento.hidden = hayRendimientos;
        resumen.hidden = false;
        estado.textContent = "";
    } catch (error) {
        estado.textContent = "No se pudieron cargar las estadísticas de la sede.";
        console.error(error);
    }
}

cargarEstadisticas();