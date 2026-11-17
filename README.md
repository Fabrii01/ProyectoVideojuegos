# Operación Rescate

Juego 3D en Unity sobre rescate médico en zonas de desastre, hecho como proyecto universitario y basado en el **ODS 3: Salud y Bienestar**.

Eres un rescatista: recorres zonas afectadas, reúnes suministros, esquivas o enfrentas peligros y curas a los heridos. Cada paciente tiene una enfermedad real, y al curarlo aprendes cómo reconocerla y tratarla. Después, en el Hub, una instructora te hace preguntas sobre lo aprendido.

| Misión | Ambiente | Enfermedades |
|---|---|---|
| 1. Terremoto en el centro | Ciudad derrumbada | Crisis de asma por polvo, riesgo de tétanos |
| 2. Incendio en la reserva | Bosque en llamas con un río | Intoxicación por humo, golpe de calor |
| 3. Emergencia en Selene | Base lunar con gravedad baja | Hipoxia, enfermedad por descompresión |

---

## Antes de empezar: lee esto

> **No uses "Code → Download ZIP".**
> Los modelos, texturas, música y fuentes (unos 470 MB) se guardan con **Git LFS**. El ZIP de GitHub no los incluye: trae archivos de texto vacíos en su lugar y el juego se abre incompleto, sin personajes ni escenarios.
>
> Sigue los pasos de abajo: con **GitHub Desktop** o con **Git + Git LFS**.

---

## Paso 1. Instalar lo necesario

1. **Unity Hub**: <https://unity.com/download>
2. **Unity 6000.5.9f1**: el proyecto usa exactamente esta versión.
   - En Unity Hub ve a **Installs → Install Editor** y elige `6000.5.9f1`.
   - Si no aparece en la lista, búscala en el archivo de versiones: <https://unity.com/releases/editor/archive>.
   - No hace falta agregar módulos extra para jugar en el editor.
3. **Una forma de descargar el proyecto**, a elegir:
   - **Opción A, la más fácil: GitHub Desktop** (<https://desktop.github.com>). Descarga los archivos de LFS sin hacer nada más.
   - **Opción B: Git** (<https://git-scm.com>). En Windows, Git ya trae Git LFS incluido.

## Paso 2. Descargar el proyecto

### Opción A: GitHub Desktop

1. En la página del repositorio, pulsa **Code → Open with GitHub Desktop**.
2. Elige una carpeta **con una ruta corta**, por ejemplo `C:\Proyectos`.
3. Pulsa **Clone** y espera a que termine. Son unos 470 MB.

### Opción B: Git desde la terminal

Abre una terminal (en Windows, *Git Bash* o *PowerShell*) en la carpeta donde quieras el proyecto y ejecuta:

```bash
git lfs install
git clone https://github.com/Fabrii01/ProyectoVideojuegos.git
```

`git lfs install` solo hace falta una vez por computador.

### ¿Ya lo descargaste y está incompleto?

Entra a la carpeta del proyecto y descarga lo que falta:

```bash
git lfs install
git lfs pull
```

**Cómo saber si te pasó:** abre cualquier `.fbx` o `.png` de `Assets` con el Bloc de notas. Si ves un texto que empieza con `version https://git-lfs.github.com/spec/v1`, ese archivo no se descargó.

## Paso 3. Abrir en Unity

1. Abre **Unity Hub** y ve a **Projects → Add → Add project from disk**.
2. Selecciona la carpeta `ProyectoVideojuegos`, la que contiene `Assets`, `Packages` y `ProjectSettings`.
3. Ábrelo con la versión **6000.5.9f1**.
4. **La primera vez tarda varios minutos**, porque Unity reconstruye su carpeta `Library`. Es normal, no lo cierres.

## Paso 4. Jugar

1. En la ventana **Project**, abre `Assets/Scenes/MenuPrincipal.unity`.
2. Pulsa **Play**.

No hace falta montar nada: las escenas ya vienen listas en el repositorio.

---

## Controles

| Tecla | Acción |
|---|---|
| W A S D | Caminar |
| Shift | Correr |
| Espacio | Saltar |
| Ratón | Mover la cámara |
| E | Hablar, abrir menús, avanzar diálogos y abrir cajas |
| Clic izquierdo o F | Atacar (necesitas el bate) |
| Esc | Pausa |

## Cómo se juega

1. **Hub:** es el punto de partida. Tiene tres zonas:
   - **Puesto de mando** (este): el médico jefe te da las misiones.
   - **Tienda** (oeste): equipo que se compra con monedas y se ve sobre tu personaje.
   - **Aula de salud** (centro): la instructora te pregunta sobre las enfermedades que curaste.
   - Los vecinos que pasean te dan consejos de salud. Acércate y pulsa E para seguir la conversación.
2. **Misiones:** junta jeringas y equipajes de salud y llévalos a cada paciente. Al curarlo aparece su ficha médica. Terminar una misión desbloquea la siguiente.
3. **Mapa:** está arriba a la derecha y muestra enemigos, suministros y monedas.
4. **Tienda:**

| Objeto | Para qué sirve |
|---|---|
| Bate de defensa | Te permite atacar a los enemigos |
| Casco de rescate | +25 de salud máxima |
| Mochila de rescate | +1 de velocidad |
| Linterna de pecho | Ilumina el camino |
| Palanca de rescate | Abre las cajas de emergencia selladas (monedas y botiquín) |
| Prismáticos | Muestran a los pacientes en el mapa |
| Botiquín de cintura | +15 de salud máxima |

---

## Herramientas para desarrollar

En la barra superior de Unity aparece el menú **SaludGame**:

| Opción | Qué hace |
|---|---|
| ★ Montar juego completo | Vuelve a generar el menú, el Hub y los tres niveles |
| Borrar monedas y compras | Reinicia el progreso guardado |
| Dar 200 monedas de prueba | Para probar la tienda |
| Desbloquear todos los niveles | Para entrar directo a cualquier misión |

> **Cuidado con "Montar juego completo":** reconstruye las escenas desde el código y **borra cualquier cambio hecho a mano** en ellas. Si quieres cambiar un nivel, edita su script y luego vuelve a montar.

### Dónde está cada cosa

| Carpeta o archivo | Contenido |
|---|---|
| `Assets/Scenes` | Escenas: `MenuPrincipal`, `Hub`, `Nivel1`, `Nivel2`, `Nivel3` |
| `Assets/Scripts` | Lógica del juego: jugador, enemigos, tienda, misiones, preguntas |
| `Assets/Scripts/Editor/MontajeNivel1.cs` (y `2`, `3`) | Diseño de cada nivel |
| `Assets/Scripts/Editor/MontajeSalud.cs` | Enfermedades, fichas médicas y preguntas |
| `Assets/Scripts/Editor/MontajeAula.cs` | Aula de salud y conversaciones de los vecinos |
| `Assets/Scripts/Editor/MontajeEstilo.cs` | Tipografías y estilo visual de la interfaz |
| `Assets/Generados` | Recursos que crea el montaje (materiales, mapas, iconos) |
| `Assets/Externos` | Modelos, texturas, música y fuentes de terceros |

---

## Problemas frecuentes

**Faltan personajes, edificios o texturas, o hay errores al importar.**
No se descargaron los archivos de LFS. Ejecuta `git lfs install` y luego `git lfs pull` dentro de la carpeta del proyecto, o vuelve a clonarlo con GitHub Desktop.

**`git lfs pull` falla con un mensaje sobre "quota" o "bandwidth".**
El repositorio agotó su cuota mensual de descarga de LFS en GitHub. Hay que esperar al mes siguiente o pedirle al dueño del repositorio que la revise en **Settings → Billing**.

**Unity pide otra versión o muestra muchos errores al abrir.**
Ábrelo con **6000.5.9f1**. Con otra versión puede convertir el proyecto y romper cosas.

**Al abrir aparece una escena vacía.**
Unity abrió una escena nueva por defecto. Abre `Assets/Scenes/MenuPrincipal.unity`.

**La primera apertura tarda mucho.**
Es normal: Unity reconstruye la carpeta `Library`, que no se sube a GitHub. Las siguientes aperturas son rápidas.

**Errores por rutas largas en Windows.**
Clona el proyecto en una carpeta con una ruta corta, por ejemplo `C:\Proyectos`.

---

## Créditos

Los modelos, cielos, texturas y música usan la licencia **CC0** (dominio público). Las tipografías usan la **SIL Open Font License**. El detalle está en [`Assets/Externos/CREDITOS.txt`](Assets/Externos/CREDITOS.txt).

- Modelos: [Kenney](https://kenney.nl), [Quaternius](https://quaternius.com) y paquetes de la Unity Asset Store incluidos en el proyecto
- Cielos y texturas: [Poly Haven](https://polyhaven.com)
- Música: [OpenGameArt](https://opengameart.org)
- Tipografías: Saira Stencil One, Barlow y Barlow Condensed, de [Google Fonts](https://fonts.google.com)

Los consejos de salud del juego son primeros auxilios básicos con fines educativos. **No reemplazan la atención de un profesional de salud.**
