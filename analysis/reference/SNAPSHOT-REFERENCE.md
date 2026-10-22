# Snapshot de referencia original

`original-menu-skipped.sna` es un snapshot SNA v2 de 64 KiB destinado exclusivamente al emulador de investigación CPCEC. No forma parte del juego nativo.

Cabecera conforme al lector/escritor de `C:/Emu/CPCEC/CPCEC.C` líneas2684–2727: magic `MV - SNA`, versión2 en10, SP0100 en21, PC0100 en23, IM1 en25, GateArray8D (ROM desconectada/modo1) en40, RAM64KiB en6B y modelo6128=2 en6D. CRTC contiene valores iniciales del juego04AB. La propia inicialización original establece vídeo, interrupción y tablas.

RAM: HEAD.BI1 sin cabecera a0100–9FFF y HEAD.BI2 aA000–ADBF. Se usa el estado previo a la reubicación, porque el arranque0100 llama22E7/0462, que ejecuta LDDR original de6600–ADBF a70D8–B897 e instala ISR. Usar relocated.bin aquí duplicaría la reubicación y corrompería gráficos.

Única modificación de referencia: bytes2F24–2F25 de `3E 99` a `37 C9` (SCF;RET). La rutina de menú2F24 devuelve carry en la opciónNuevaPartida (`2FF6–2FFB`), por lo que el llamador011F/0122 ejecuta22ED. El parche evita interacción de teclado sin cambiar movimiento, salas, gráficos, enemigos ni reglas. No constituye evidencia de ejecución hasta que el snapshot se abra y capture.
