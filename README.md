# ⚔️ RiftKeeper - MVP

Bienvenido al repositorio de desarrollo de **RiftKeeper**. Este es un juego Roguelike Top-Down desarrollado en Unity, donde exploramos la transición entre épocas Medievales y Futuristas.

Este documento te guiará para configurar el proyecto en tu computadora y colaborar sin conflictos usando GitHub.

---

## 🛠️ 1. Requisitos Previos

Antes de empezar, asegúrate de tener instalado lo siguiente:

1.  **Unity Hub:** [Descargar aquí](https://unity.com/download).
2.  **Unity Editor (Versión Específica):**
    * Abre el archivo `ProjectSettings/ProjectVersion.txt` en este repo para ver la versión exacta (ej. `2022.3.10f1`).
    * Instala **EXACTAMENTE** esa versión desde Unity Hub para evitar conflictos de sistema.
3.  **GitHub Desktop:** [Descargar aquí](https://desktop.github.com/). Nuestra herramienta para subir y bajar cambios.
4.  **Visual Studio:** (O tu editor de código preferido) para editar los scripts C#.

---

## 🚀 2. Configuración Inicial (Cómo abrir el proyecto)

Sigue estos pasos la primera vez que descargues el proyecto:

1.  **Clonar el Repositorio:**
    * Abre GitHub Desktop.
    * Ve a `File > Clone Repository`.
    * Selecciona `RiftKeeper-MVP` y elige una carpeta en tu PC.
2.  **Agregar a Unity Hub:**
    * Abre Unity Hub -> Pestaña `Projects`.
    * Haz clic en `Open` (o `Add`).
    * Selecciona la carpeta raíz que acabas de clonar.
3.  **Abrir el Proyecto:**
    * Haz clic en el proyecto en la lista.
    * 🛑 **Paciencia:** La primera vez tardará varios minutos porque Unity debe reconstruir la carpeta `Library` (que no subimos a GitHub para ahorrar espacio).

---

## 🤝 3. Flujo de Trabajo Colaborativo (¡LEER ATENTAMENTE!)

Para evitar borrar el trabajo de otros, seguimos estas reglas. Usaremos **Ramas (Branches)** para trabajar de forma segura.

### ⛔ Regla de Oro: NUNCA trabajes directamente en la rama `main`.

La rama `main` es la versión "funcional y limpia" del juego. Solo la tocamos para unir trabajo terminado.

### Paso a Paso para Trabajar:

#### 1. Crea tu Rama (Branch)
Antes de mover un solo dedo en Unity:
* Abre GitHub Desktop.
* Asegúrate de estar en `Current Branch: main`.
* Haz clic en `Fetch origin` para tener la última versión.
* Haz clic en `New Branch`.
* **Nombra tu rama descriptivamente:**
    * Si es una nueva función: `feature/nombre-funcion` (ej: `feature/ia-enemigo`, `feature/nuevo-tilemap`).
    * Si es un arreglo: `fix/nombre-error` (ej: `fix/velocidad-jugador`).

#### 2. Trabaja en Unity
* Abre Unity (asegúrate de que GitHub Desktop muestre tu nueva rama).
* Haz tus cambios, crea scripts, pinta niveles, etc.
* 💾 **Guarda todo:** Guarda la Escena (`Ctrl+S`) y guarda los Scripts en Visual Studio.

#### 3. Sube tus Cambios (Commit & Push)
* Vuelve a GitHub Desktop. Verás una lista de archivos modificados.
* En el cuadro de abajo a la izquierda:
    * **Summary:** Título breve (ej: "Añadida lógica de embestida al enemigo").
    * **Description:** Detalles extra si son necesarios.
* Haz clic en **Commit to [tu-rama]**.
* Haz clic en **Push origin** (arriba a la derecha) para subir tu rama a la nube.

#### 4. Solicita Unir tus Cambios (Pull Request)
Cuando termines tu tarea y quieras que esté en el juego final:
* En GitHub Desktop, aparecerá un botón **"Create Pull Request"**. Haz clic en él.
* Se abrirá el navegador. Revisa los detalles y confirma.
* Avísale al líder del proyecto (o a un compañero) para que revise tu código y apruebe la unión (Merge).

---

## 📂 Estructura del Proyecto

Para mantener el orden, usamos estas carpetas clave en `Assets/`:

* **`_GameManagers`**: Prefabs y scripts de gestión global (WaveManager, GameManager).
* **`Scripts/`**: Todo el código C#.
* **`Sprites/`**: Imágenes en bruto (.png). Recuerda configurar el PPU y Filter Mode a "Point".
* **`Prefabs/`**: Objetos pre-configurados (Enemigos, Proyectiles). **Usa Prefabs siempre que puedas** en lugar de objetos sueltos en la escena.
* **`Tiles/`**: Los archivos `.asset` de los tiles (Suelo, Muros).
* **`Scenes/`**: Las escenas del juego.
