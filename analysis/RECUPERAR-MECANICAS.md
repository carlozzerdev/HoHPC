# Cómo recuperar las mecánicas del original

El DSK contiene el programa y sus datos. Recuperar dibujos y habitaciones no
convierte automáticamente las reglas Z80 a C#: hay que identificar y traducir
las rutinas, su estado y sus dependencias. Esa traducción quedó parcial en este
prototipo. El audio anterior, además, era una sustitución nueva; no provenía del
reproductor CPC. Un informe de pruebas que dibuja 301 salas no demuestra que se
puedan completar los puzles de las 301 salas.

## Trabajo reproducible ya disponible

Desde la carpeta del proyecto:

```
python tools/recover.py
python tools/audit_behaviors.py
python tools/recover_audio.py
python tools/verify_audio.py
Compilar.cmd
```

El primer script lee el disco, valida archivos AMSDOS, reconstruye las copias del
cargador en memoria y extrae salas, objetos, colisiones, animaciones, tablas de
comportamiento y rutas de teletransporte. No interpreta instrucciones Z80.

El segundo contrasta instrucciones con los bytes del disco y genera
`mechanics-evidence.json`: inventario de los 37 despachos, direcciones, número de
objetos de cada familia y estado nativo **parcial/pendiente**. El inventario incluye
también el caso 0, sin despacho. `BEHAVIOR-AUDIT.md` explica las diferencias concretas.

Los scripts de audio extraen las tablas originales y comprueban once vectores
iniciales AY, generación de PCM y final de
los efectos. Las pruebas no certifican por sí solas identidad sonora durante toda
una partida. Ver `original-audio.md`.

## Lo que hay que observar y traducir por cada familia

1. Elegir una sala y una entidad concreta, conservando su tipo, posición,
   descriptor y dirección de comportamiento. No asignar funciones por el dibujo.
2. Seguir la tabla `387F` hasta su rutina, incluidos los auxiliares llamados. Leer
   qué campos modifica: XYZ, dirección, temporizadores y máscaras de contacto.
3. Ejecutar el original en un emulador CPC con depurador, solo como referencia de
   desarrollo. Guardar un estado reproducible justo antes del contacto o salto.
4. Aplicar una entrada controlada, avanzar un tick lógico y registrar antes/después.
   Repetir contra suelo, pared, otro cuerpo y los dos personajes. Una grabación de
   vídeo ayuda visualmente; una traza de memoria permite recuperar los incrementos.
5. Traducir la regla a C# y comparar ambos recorridos tick a tick. Mantener visibles
   los casos todavía desconocidos. No reemplazarlos por un movimiento genérico.

Para el audio, registrar los cambios AY R0–R13 y sus instantes en la referencia.
Comparar esa secuencia con el secuenciador nativo; después cotejar el PCM. La música,
la prioridad entre efectos, el mezclador y el ruido también necesitan validación.

El programa nativo entregado continúa sin emulador ni intérprete Z80. El emulador
se usa para obtener evidencia de desarrollo, si se instala uno con depurador.

## Correcciones de esta revisión

- **Muelle:** `2A41–2A6F` reconoce soporte sprite 43/44. Suma un nivel de salto:
  Heels pasa de 4 a 8 ticks; Head, unidos o Heels con mejora alcanzan 10, por el
  límite explícito que sustituye 12 por 10. No hay rebote automático sin pulsar
  salto. `4D98` comprime el muelle y reproduce su recuperación 43,44,43,44,43.
- **Ascensor:** conductas 18/23 inicializan auxiliar 52/90 y pasan a 16. `4E82`
  mueve una unidad Z original por tick entre los límites codificados. El motor
  nativo transporta al personaje apoyado y comprueba que no atraviese un sólido.
- **6160 → 6150:** el ascensor real está en 6160, XY 6,3. La sala 6150 tiene suelo
  tipo 7 y bloques de llegada. `21D8` verifica la abertura superior, `25BF` detecta
  Z<132 y la transición resta una planta; `2BB5` determina la altura de entrada.
  Al caminar fuera de los bloques de 6150 se cae de nuevo por el hueco hacia 6160.
- **Enemigos:** 33 recupera persecución diagonal cercana; 15 elige el eje menor
  no nulo y favorece X en empate. Las familias aleatorias consultan el generador
  antes de la gravedad. Persisten diferencias en máscaras, inicialización y otras
  familias; el inventario no las presenta como resueltas.
- **Sonido:** se sustituye el barrido inventado por tablas del disco y un
  secuenciador nativo AY. La síntesis analógica y el orden completo de prioridades
  aún no se han contrastado con una traza del original.

Limitaciones de esta implementación: la pausa de extremos del ascensor usa un
contador por entidad; el original comparte `4E81`. Las cadenas verticales de
objetos y todas las variantes de suelo/daño de conductas 6/7 requieren más casos.
No se han añadido destinos ausentes ni se ha certificado una partida completa.

`build/actual/verification` contiene los resultados y capturas de la plataforma
arriba (`lift-6160-top.png`) y de la llegada (`lift-6150-arrival.png`).
