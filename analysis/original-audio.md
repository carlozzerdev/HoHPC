# Sonido CPC: recuperación y límites

El motor anterior `GameAudio.Tone` creaba barridos de frecuencia a mano; no contenía sonidos extraídos del original. Se ha sustituido por una traducción nativa de su secuenciador y una síntesis AY de tres canales. No ejecuta instrucciones Z80 ni utiliza un emulador durante el juego.

`tools/recover_audio.py` reconstruye la memoria desde el disco suministrado mediante el cargador ya recuperado y genera `src/OriginalAudioData.cs`. Conserva exactamente los bytes `0B8B–149F`, SHA256 `3e71327401065b1f7b072f803566544b6baa4f2a8bb5e757aa4cc8f60b71ec81`. Esta región incluye tablas y código entre ellas; el motor únicamente lee las tablas y no ejecuta los bytes.

Evidencia del programa original:

- `0B60–0B8A` escribe los registros AY 0–10 desde `10A2`; `04D4` invoca el secuenciador con divisor de interrupción 6 (`04CE`), equivalente a 50 actualizaciones por segundo.
- `0D91–0DEA` interpreta el comando como canal (dos bits superiores) e índice (seis inferiores), consulta las tablas de `124C` y mantiene la identidad del efecto para no reiniciarlo mientras permanece solicitado.
- `0DEB–0E32` selecciona los tres canales desde la tabla `1111`, utilizada por los efectos de muerte.
- `0C71–0D76`, `0E33–104D` manejan notas, duración, volumen por patrones, vibrato, deslizamiento y ruido.
- Salto: `2A72`, `2B21`, `2B28` seleccionan `85`. Aterrizaje: `283D` selecciona `88`. Muelle: `4DB6`, `2B1C` seleccionan índice `04` del canal B (`44`). Plataforma: `4E82` selecciona `41`. Teletransporte: `4C23` selecciona `47`. Daño: `476E` selecciona el grupo `C6`. Las direcciones mayores pueden requerir consultar `relocated.bin`, no la interpretación lineal previa de BI1.

Correspondencias adicionales: `3F84: 06 C5 C3 91 0D` es la continuación genérica al recoger objetos; `3FAF: 06 C2 C3 91 0D` se utiliza al adquirir equipamiento/munición; `0271: 06 C4 C3 91 0D` es el efecto de interacción usado por `2B19` al transportar. `25F6–262B` construye el proyectil en `2499`, consume munición mediante `4143` y llama al sonido `48` (`2629: 06 48 CD 91 0D`). El efecto `42` de `37E1–37EE` pertenece a animaciones de objetos y no se utiliza como sustituto del disparo.

`python tools/verify_audio.py` contrasta los primeros registros AY de once efectos con una decodificación independiente desde el disco. También ejecuta 2.000 frames por efecto y comprueba generación PCM y silencio al final. Verifica que las solicitudes repetidas no reinicien el efecto, que los efectos de grupo bloqueen solicitudes de canal y que el comando de parada silencie el canal. No es una comparación diferencial de todos los frames contra el driver original.

Límites: las notas y patrones proceden del disco, pero la fidelidad auditiva completa aún requiere cotejar trazas AY del original o grabaciones controladas. La síntesis usa el reloj CPC de 1 MHz y niveles logarítmicos aproximados; no reproduce el circuito analógico, filtros ni la fase inicial física del generador de ruido. La planificación de prioridades y cortes del motor de juego no está recuperada completamente. Los eventos sin asignación verificada (pasos y cambiar personaje) quedan en silencio en lugar de conservar sonidos inventados. No se incluye todavía la música de presentación.

## Pasos y ritmo de salida

`2986–2988` solicita 80 mediante 2B21 al caminar; `2B28` lo envía al canal C.
Se ha asignado a `SoundCue.Step`. El motor emite la solicitud cada tick con
avance en suelo. Al parar usa una parada selectiva de ese efecto; si en el canal
ya se reproduce un salto u otro sonido, no lo corta. Pausa y visor paran los pasos.
El verificador incluye ahora 11 vectores iniciales independientes y comprueba
que detener pasos no interrumpe el salto.

La lentitud detectada no era el reloj del secuenciador: el código esperaba a que
Windows terminase un único bloque antes de presentar el siguiente. En este equipo
la prueba de posición waveOut midió 11.086,6 muestras/segundo para un formato de
22.050. Con tres bloques de 441 muestras en cola y CALLBACK_EVENT la medida vuelve
a aproximadamente 22.050 muestras/segundo. Los registros AY siguen avanzando cada
441 muestras, es decir 50 veces/segundo. No se cambia el tono ni la duración de los
datos para compensar un fallo de reproducción.

`--verify-audio-timing` mide tres segundos en silencio mediante waveOutGetPosition
(TIME_SAMPLES), tras 400 ms de calentamiento. Informa error si la diferencia con
22.050 supera el 5 %. Los resultados anteriores y posteriores están en
`audio-output-verification.json`. No es una comparación auditiva contra el CPC.
