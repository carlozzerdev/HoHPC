# Head over Heels — reconstrucción nativa para PC

Estado: **prototipo técnico, no réplica completa del juego**.

La aplicación Windows dibuja datos extraídos de `Head over Heels (Europe).dsk`
y traduce reglas de movimiento, salto, unión, bolsa y disparo a C#. No contiene un
intérprete Z80, emulador de CPC, ROM ni dependencia de un emulador instalado.

## Ejecutar

Abrir `build/HeadOverHeelsPC.exe` o `Jugar.cmd`. Requiere Windows y .NET Desktop
Runtime 10, disponible en el equipo utilizado para compilar.

- WASD o flechas: movimiento paralelo a las paredes. Arriba/W: arriba-derecha;
  abajo/S: abajo-izquierda; izquierda/A: arriba-izquierda; derecha/D: abajo-derecha.
- Espacio: salto.
- Espacio sobre una plataforma de teletransporte: activar su ruta original.
- Tab: cambiar de personaje.
- Tab cuando Head está sobre Heels: unirlos; Tab de nuevo: separarlos.
- E: recoger/dejar un objeto con la bolsa de Heels.
- F o Ctrl: disparar con Head, si tiene bocina y munición.
- R: nueva partida.
- M: activar/silenciar los efectos de sonido sintetizados.
- F2: vista de nueve posiciones de habitación en la misma planta.
- RePág / AvPág: recorrer las habitaciones recuperadas en el visor.
- Esc: pausa. F11: pantalla completa.

El mapa es un visor de datos: una posición vacía indica que no hay una habitación
recuperada en esas coordenadas. Hay reglas de movimiento discreto, colisiones,
inventario, daño y varias familias de enemigos portadas desde el Z80; todavía
necesitan contrastarse durante partidas completas.

## Recuperación reproducible

`python tools/recover.py` lee el DSK original y genera `assets/world.json`,
`assets/provenance.json` y `analysis/relocated.bin`. Usa solamente Python 3 y
su biblioteca estándar. No ejecuta código del juego.

Se recuperan cuatro archivos AMSDOS, 301 registros de habitaciones con objetos
y macros expandidas, 100 entradas gráficas, 52 registros de coleccionables,
paredes y ocho paletas originales. Los identificadores
y direcciones originales se conservan para contrastar cada hallazgo.

## Compilar y comprobar

Ejecutar `Compilar.cmd`. Requiere Python 3 y el SDK de .NET 10.

`build/HeadOverHeelsPC.exe --verify` dibuja todas las habitaciones, comprueba
salto/aterrizaje nativos y separación de personajes, y guarda imágenes e informe
en `build/verification`. Son comprobaciones de funcionamiento del prototipo;
**no demuestran equivalencia con el original**.

## Estructura

- `src/World.cs`: datos inmutables y estado independiente por habitación.
- `src/NativeGame.cs`: personajes, colisiones, inventario, daño y proyectiles.
- `src/ObjectBehaviors.cs`: familias de movimiento recuperadas.
- `src/OriginalRandom.cs`: generador aleatorio original traducido a C#.
- `src/Renderer.cs`: dibujo por habitación, independiente de la simulación.
- `src/Program.cs`: ventana, entrada de teclado y composición de varias cámaras.
- `tools/recover.py`: lectura del disco y conversión estática de datos.
- `analysis/INVESTIGACION.md`: evidencia, límites y trabajo pendiente.

## Pendiente para replicar el juego

Faltan cambios verticales por huecos/caídas, empuje completo,
interruptores y varias familias de objetos/plataformas, sonido y menús originales.
El suelo usa una rejilla provisional y la selección de variantes de pared y
partes de puerta está incompleta. La colisión compuesta, los checkpoints y la
respuesta exacta a todas las interacciones necesitan más reconstrucción.

Se portaron el ciclo nominal de 80 ms, pasos distintos de Head/Heels, ascenso
discreto, unión, bolsa, requisitos de bocina/munición y congelación de enemigos
elegibles. Los coleccionables y coronas se leen de su tabla independiente. Las
familias móviles implementadas cubren rebotes, giros, cambios aleatorios y
persecución; no representan todavía todos los 37 comportamientos originales.

Las 482 puertas visibles recuperadas tienen destino y retorno comprobados.
Se recuperaron 22 parejas de teletransporte; se conserva el orden original
de búsqueda y los contadores de 12 ticks de salida y 8 de llegada. Estas pruebas
comprueban los datos y reglas portados, sin certificar todavía una partida completa.

`analysis/CHARACTERS-Z80.md`, `OBJECTS-Z80.md`, `SPRITE-COLOR-Z80.md`,
`BEHAVIORS-Z80.md` y `PROJECTILES-Z80.md` registran direcciones y límites.
La referencia emulada en `analysis/reference` se utiliza solo durante desarrollo.

Las habitaciones tienen estado propio y el renderizador recibe una habitación
explícita. Por ello la futura vista de varias salas no depende de alterar una
pantalla o memoria global de CPC.

Los gráficos y datos se obtienen del disco facilitado por el usuario. Créditos
del original: Jon Ritman, Bernie Drummond, Guy Stevens y Ocean Software.

## Audio nativo

Efectos sintetizados para pasos, salto, aterrizaje, disparo, recogidas, da?o,
teletransporte, cambio de personaje y bolsa. Se mezclan hasta ocho efectos
simult?neos mediante la salida de audio de Windows, sin bloquear la simulaci?n.
Son efectos nuevos de estilo retro; todav?a no reproducen el sonido del CPC.
M permite silenciarlos. La cabecera muestra si el audio no se puede abrir.

## Correcci?n de perspectiva

Abrir `Jugar-perspectiva.cmd` para la compilaci?n con audio y orden de dibujo
basado en los vol?menes de objetos y personajes. Se comprueban las pilas de
escalones y el personaje sobre el teletransportador; las escenas reproducibles
se guardan como `depth-stairs.png` y `depth-teleporter.png` con `--verify`.
Las intersecciones complejas con ciclos conservan un desempate estable; esta
correcci?n no certifica toda la ocultaci?n del juego original.

Todos los lanzadores `Jugar*.cmd` abren ahora el mismo ejecutable actualizado
en `build/HeadOverHeelsPC.exe`. `Compilar.cmd` tambi?n genera esa misma ruta.
