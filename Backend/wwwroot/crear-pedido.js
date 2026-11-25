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

formulario.addEventListener("submit", (evento) => evento.preventDefault());

botonAgregar.addEventListener("click", () => {
    const fila = agregarProducto();
    fila.querySelector('select[name="codigoProducto"]').focus();
});

cargarProductos();
