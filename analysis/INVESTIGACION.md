# Investigación del disco suministrado

## Identidad y herramientas

DSK: `Head over Heels (Europe).dsk`, 194816 bytes.
SHA-256: `ebea9f40e43763f1d44d16766635a55505682e535b2712e1d60451f0a657b3fd`.
Extended CPC DSK, 40 pistas, una cara, DATA, sectores C1–C9 de 512 bytes.

Plataforma de referencia indicada por el usuario: **Amstrad CPC 6128, CPU Z80,
128 KiB de RAM con bancos de 16 KiB**. El contenedor DSK es legible y ya se ha
extraído correctamente. Su rechazo por el inspector genérico de REA no implica
un fallo del disco ni impide desensamblar los archivos Z80 extraídos.

Se consultó la habilidad REA versión 42 y se ejecutó REA CLI 6.5.0.
`doctor --client codex --json` detectó registro alineado pero ningún motor
Hopper/Ghidra/IDA disponible. No hay herramientas REA MCP conectadas en la sesión.
`inspect-artifact` rechazó este DSK como formato no soportado; su salida literal
está en `rea-disk.json`. **No existe un Evidence ID de análisis exitoso del juego
por REA**. No se ha cambiado la configuración ni instalado motores.

Los hallazgos siguientes proceden de extracción y análisis estático local,
no de ejecución observada ni de descompilación REA. Se utilizaron como apoyo
los ejecutables existentes cpcdskex y binspy en `C:/IA/cpctools/publish`.
El recuperador del proyecto es autónomo y sus resultados de extracción se
contrastaron con cpcdskex.

## Observaciones confirmadas en bytes

| Archivo | Contenido | Carga AMSDOS | Entrada |
| --- | ---: | ---: | ---: |
| HEAD | 890 bytes | 0170 | 0000 |
| HEADIMA.POW | 6856 bytes | 8C38 | 8C38 |
| HEAD.BI1 | 40704 bytes | 0100 | 0100 |
| HEAD.BI2 | 3520 bytes | A000 | 0000 |

`loader/basic.txt` contiene el BASIC decodificado. Incluye preguntas en francés
para velocidad, equipo, disparos, invulnerabilidad y vidas. Los POKEs opcionales
están en 063E–0640, 2972, 2B32, 4C45 y 2BC5 antes de la reubicación del cargador.
La recuperación no aplica esos POKEs. Esto no demuestra que todos los bytes del
disco coincidan con una edición comercial sin modificaciones.

El BASIC carga BI1 en 0600 y copia 9F00 bytes a 0100. Carga BI2 en el banco
seleccionado por 7FC4 y copia 0DC0 bytes a A000. La rutina 0462 copia 47C0 bytes
desde 6600–ADBF a 70D8–B897, hacia atrás. `recover.py` aplica esas copias como
transformaciones de datos, sin ejecutar la CPU.

La memoria adicional del 6128 es relevante para el cargador: C4 permite alojar
BI2 en la ventana 4000 mientras BI1 se carga en la RAM principal con C0. La
imagen de 64 KiB recuperada representa los datos después de esas copias, no
toda la RAM del CPC 6128. `game_data_image` conserva explícitamente esta
distinción. El motor final sigue siendo nativo; este análisis de bancos no
introduce emulación en el ejecutable.

## Habitaciones

- Tablas de registros: 5241 y 60E6; longitud de un byte y salto longitud+1.
- Clave: byte XY (X bajo, Y alto) y nibble de planta; ID `(XY<<8)|(planta<<4)`.
- Macros: registros en 50D0, claves C0–DB, invocados desde el flujo de objetos.
- Decodificación MSB primero: rutina 22B2.
- Lectura de cabecera y expansión de objetos: 1FB8–22CD.
- Tabla de ocho formas y límites: 1ECE, cuatro bytes por forma.
- Descriptores de tipo: 38C9, tres bytes por entrada (36F5).
- Lista variable de posiciones: termina con X=7, Y=7, Z=0 (228B–2294).
- Llamadas a macros: desplazamiento X/Y con signo de tres bits y Z−7 (2039).
- Las claves 8940 y 8A40 aparecen en la inicialización 22ED–2321.

El recuperador obtiene 301 claves únicas y comprueba que cada flujo de habitación
no sobrepase su registro. La interpretación de campos como tema, área y tipo de
puerta es provisional; se conservan sus números originales.

## Gráficos

La selección de gráficos se encuentra en 35B3–36BD, después de la reubicación.
Se separan imagen y máscara de un bit:

| Índices | Base | Paso | Píxeles |
| --- | --- | --- | --- |
| 0–15 | 8270 | 336 | 24 × 56 |
| 16–23 | 8A50 | 192 | 24 × 32 |
| 24–83 | 8BD0 | 144 | 24 × 24 |
| 84–99 | AA30 | 224 | 32 × 28 |

La inspección de `build/verification/sprites.png` permite reconocer puertas,
personajes, objetos y bloques. Algunos índices parecen patrones auxiliares;
no se han clasificado todas las entradas. Las animaciones se resuelven a un
fotograma inicial; su temporización y comportamiento están pendientes.

## Motor nativo y límites

El ejecutable usa JSON convertido y C#. Ningún opcode Z80 se ejecuta durante
el juego. Cada RoomState contiene sus entidades y Renderer.Room recibe su sala
como argumento. El visor puede componer nueve salas sin modificar su estado.

El salto, movimiento, colisiones y transiciones actuales son decisiones
provisionales de implementación. No se han portado todas las rutinas originales
ni comparado partidas. No se presenta el prototipo como una réplica terminada.

## Referencia externa localizada, no incorporada

Se localizó el desensamblado CPC de fphenix:
https://github.com/fphenix/HeadOverHeels_CPC . No se pudieron descargar sus
archivos en este entorno. No se ha usado su código para construir el motor.

## Comprobaciones

Ver `build/verification/results.json`. Se dibujan las 301 habitaciones y se
comprueban inventario, salas iniciales, salto/aterrizaje y personajes separados.
La validación de fidelidad permanece explícitamente pendiente.
