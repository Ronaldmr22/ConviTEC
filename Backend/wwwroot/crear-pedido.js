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
