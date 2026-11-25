const formulario = document.querySelector("#formulario-pedido");
const lineas = document.querySelector("#lineas");
const plantilla = document.querySelector("#plantilla-producto");
const botonAgregar = document.querySelector("#agregar-producto");

function agregarProducto() {
    const fila = plantilla.content.firstElementChild.cloneNode(true);

    fila.querySelector(".quitar-producto").addEventListener("click", () => {
        if (lineas.childElementCount > 1) {
            fila.remove();
            botonAgregar.focus();
        }
    });

    lineas.append(fila);
    return fila;
}

formulario.addEventListener("submit", (evento) => evento.preventDefault());

botonAgregar.addEventListener("click", () => {
    const fila = agregarProducto();
    fila.querySelector("input").focus();
});

agregarProducto();