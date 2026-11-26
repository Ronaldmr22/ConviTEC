const formulario = document.querySelector("#formulario-pedido");
const lineas = document.querySelector("#lineas");
const plantilla = document.querySelector("#plantilla-producto");
const botonAgregar = document.querySelector("#agregar-producto");
const estadoCatalogo = document.querySelector("#estado-catalogo");
const codigosProducto = [];

function agregarProducto() {
    const fila = plantilla.content.firstElementChild.cloneNode(true);
    const selector = fila.querySelector('select[name="codigoProducto"]');

    for (const codigo of codigosProducto) {
        selector.add(new Option(codigo, codigo));
    }

    fila.querySelector(".quitar-producto").addEventListener("click", () => {
        if (lineas.childElementCount > 1) {
            fila.remove();
            resumenPedido.hidden = true;
            botonAgregar.focus();
        }
    });

    lineas.append(fila);
    return fila;
}

async function cargarProductos() {
    try {
        const respuesta = await fetch("/api/inventario-global");

        if (!respuesta.ok) {
            throw new Error(`GET /api/inventario-global devolvió HTTP ${respuesta.status}`);
        }

        const registros = await respuesta.json();

        for (const registro of registros) {
            if (!codigosProducto.includes(registro.codigoProducto)) {
                codigosProducto.push(registro.codigoProducto);
            }
        }

        if (codigosProducto.length === 0) {
            estadoCatalogo.textContent = "El catálogo global aún no tiene productos.";
            return;
        }

        agregarProducto();
        botonAgregar.disabled = false;
        estadoCatalogo.textContent = "";
    } catch (error) {
        estadoCatalogo.textContent = "No se pudieron cargar los productos.";
        console.error("Error al preparar el selector de productos:", error);
    }
}

const resumenPedido = document.querySelector("#resumen-pedido");
const resumenDestino = document.querySelector("#resumen-destino");
const resumenProductos = document.querySelector("#resumen-productos");
const destino = document.querySelector("#destino");
const botonRegistrar = document.querySelector("#registrar-pedido");
const estadoPedido = document.querySelector("#estado-pedido");

botonRegistrar.addEventListener("click", async () => {
    if (resumenPedido.hidden || !formulario.reportValidity()) {
        return;
    }

    const productos = [];

    for (const fila of lineas.children) {
        productos.push({
            codigoProducto: fila.querySelector('[name="codigoProducto"]').value,
            cantidad: Number(fila.querySelector('[name="cantidad"]').value)
        });
    }

    // El selector entrega texto, por ejemplo "LIB" o "7".
    const pedido = { destino: destino.value, productos };

    formulario.inert = true;
    estadoPedido.textContent = "Enviando pedido…";

    try {
        const respuesta = await fetch("/api/pedidos", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(pedido)
        });

        if (respuesta.status === 404 || respuesta.status === 405) {
            estadoPedido.textContent =
                "El registro de pedidos todavía no está disponible en esta sede.";
            return;
        }

        const resultado = await respuesta.json();

        if (resultado.aceptado === false) {
            estadoPedido.textContent = resultado.mensaje || "El pedido fue rechazado.";
            return;
        }

        if (!respuesta.ok || resultado.aceptado !== true || !resultado.idPedido) {
            throw new Error(`Respuesta inesperada de POST /api/pedidos: HTTP ${respuesta.status}`);
        }

        // Limpia el formulario cuando el backend confirma el registro.
        estadoPedido.textContent = `Pedido registrado: ${resultado.idPedido}`;
        formulario.reset();
        lineas.replaceChildren();
        agregarProducto();
        resumenPedido.hidden = true;
    } catch (error) {
        estadoPedido.textContent =
            "No se pudo confirmar el registro. Verifica el estado del pedido antes de reintentar.";
        console.error(error);
    } finally {
        formulario.inert = false;
        (resumenPedido.hidden ? destino : botonRegistrar).focus();
    }
});

formulario.addEventListener("submit", (evento) => {
    evento.preventDefault();

    if (lineas.childElementCount === 0) {
        return;
    }

    resumenDestino.textContent = `Destino: ${destino.selectedOptions[0].textContent}`;
    resumenProductos.replaceChildren();

    // Lee los productos y las cantidades del formulario.
    for (const fila of lineas.children) {
        const producto = fila.querySelector('[name="codigoProducto"]').value;
        const cantidad = fila.querySelector('[name="cantidad"]').value;

        const detalle = document.createElement("li");
        detalle.textContent = `${producto} · ${cantidad} unidad(es)`;
        resumenProductos.append(detalle);
    }

    resumenPedido.hidden = false;
});

// Oculta el resumen anterior si se editan los datos.
formulario.addEventListener("input", () => {
    resumenPedido.hidden = true;
});

botonAgregar.addEventListener("click", () => {
    const fila = agregarProducto();
    resumenPedido.hidden = true;
    fila.querySelector('select[name="codigoProducto"]').focus();
});

cargarProductos();
