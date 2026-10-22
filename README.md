# Head over Heels — reconstrucción nativa para PC

Estado: **prototipo técnico, no réplica completa del juego**.

La aplicación Windows dibuja datos extraídos de `Head over Heels (Europe).dsk`
y traduce reglas de movimiento, salto, unión, bolsa y disparo a C#. No contiene un
intérprete Z80, emulador de CPC, ROM ni dependencia de un emulador instalado.

## Ejecutar

Abrir `build/actual/HeadOverHeelsPC.exe` o `Jugar.cmd`. Requiere Windows y .NET Desktop
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

`build/actual/HeadOverHeelsPC.exe --verify` dibuja todas las habitaciones, comprueba
salto/aterrizaje nativos y separación de personajes, y guarda imágenes e informe
en `build/actual/verification`. Son comprobaciones de funcionamiento del prototipo;
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

Faltan variantes de cambios verticales y caídas, interacciones completas entre cuerpos,
interruptores y varias familias de objetos, música y menús originales.
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

## Audio y perspectiva

Se ha sustituido el audio provisional por un secuenciador nativo y tablas de
efectos extraídas del disco CPC. Hay once asignaciones verificadas: pasos, salto,
aterrizaje, daño, teletransporte, muelle, plataforma, disparo, recogida,
equipamiento y bolsa. M permite silenciarlos. El cambio de personaje queda mudo mientras no se identifique su correspondencia
original. Los pasos se detienen al parar, saltar o pausar.
La síntesis AY y las prioridades completas todavía requieren comparación con el
original; no se presenta el audio como idéntico. Ver `analysis/original-audio.md`.

El orden de dibujo usa los volúmenes de objetos y personajes. Las escenas de
escalones y teletransportador se guardan con `--verify`; los ascensores disponen
de margen superior para que el personaje no desaparezca al subir.

`Jugar.cmd` abre `build/actual/HeadOverHeelsPC.exe`; `Compilar.cmd` genera esa
misma ruta. La carpeta permite mantener abierta la compilación anterior mientras
se prepara esta revisión. Para usar los cambios, cierra la partida anterior
cuando quieras y vuelve a abrir `Jugar.cmd`.

## Vista de juego y cámara

La cabecera compacta deja más espacio para jugar y conserva la columna del otro
personaje. Las habitaciones visitadas permanecen en un espacio isométrico común;
al cruzar una puerta la cámara se desplaza suavemente hacia la nueva habitación.
El teletransporte y el cambio de personaje también desplazan la cámara según las
coordenadas del destino. Las plantas tienen separación vertical y las coordenadas
originales de 16 sectores se desenvuelven al descubrir salas. F2 conserva el visor.
La separación entre sectores es fija: las salas pequeñas mantienen espacio entre
ellas. `--verify` comprueba orientación, estabilidad al regresar y llegada de la
cámara tras un teletransporte, y genera `camera-transition.png`.

## Empujar objetos

Camina contra un objeto móvil para empujarlo, sin tecla adicional ni bolsa.
Head, Heels y ambos unidos pueden desplazar el muelle y los otros cuerpos pasivos
identificados por sus comportamientos originales: 11 tipos, 172 objetos del mapa.
Las paredes, los obstáculos y el otro personaje bloquean el empuje. Las posiciones
se conservan en la habitación. Recoger con E sigue requiriendo la bolsa y un objeto
transportable. La inercia del original sigue siendo parcial. Evidencia y alcance
en `analysis/PUSHING-Z80.md`.

## Muelles, ascensores y salas superpuestas

Al pulsar salto sobre un muelle, Heels obtiene 8 ticks de ascenso y Head o la
pareja unida 10; también se conserva la mejora de salto de Heels y el límite de
10 ticks del original. El muelle muestra compresión y recuperación.

Las plataformas verticales de conductas 16/18/23 suben y bajan y llevan al
personaje apoyado. La plataforma de 6160 permite subir a la sala 6150: espera
hasta arriba y salta con el perro. El hueco de 6150 también permite caer a 6160.
La cámara sitúa la planta superior por encima de la inferior.

`analysis/RECUPERAR-MECANICAS.md` explica cómo se extraen y contrastan las reglas;
`analysis/BEHAVIOR-AUDIT.md` inventaría las 37 familias y sus lagunas. La revisión
corrige persecución diagonal de la familia 33, selección de eje de la 15 y orden
de consulta aleatoria. Siguen pendientes interruptores, cintas, desapariciones y
otras interacciones: dibujar todas las habitaciones no acredita el juego completo.

Los coleccionables aplican gravedad desde su posición inicial y descansan sobre
los objetos de la sala. La bocina de 3060 cae hasta la columna de altura 3 y se
recoge por contacto físico. Bolsa y bocina se adquieren para el inventario común;
su uso sigue correspondiendo respectivamente a Heels y Head. Para disparar con
Head hacen falta también munición y F/Ctrl. `--verify` comprueba la recogida en
esa sala y el disparo posterior y genera `hooter-before.png`/`hooter-collected.png`.

La salida mantiene tres bloques PCM en cola y actualiza el secuenciador a 50 Hz.
`build/actual/HeadOverHeelsPC.exe --verify-audio-timing` mide durante tres segundos
la posición real del dispositivo, sin emitir sonido, y guarda `audio-timing.json`.
Comprueba que la velocidad queda dentro del 5 % de las 22.050 muestras/segundo.
