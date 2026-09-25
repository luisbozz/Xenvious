# Glossary

| Term | Meaning |
|---|---|
| **Legacy / Enhanced** | The two PC editions of GTA V (`GTA5.exe`, `GTA5_Enhanced.exe`). Same content, different binaries and addresses. |
| **Creator** | Rockstar's in-game Content Creator. Each job type is its own script: `fm_race_creator`, `fm_lts_creator`, `fm_capture_creator`, `fm_deathmatch_creator`, `fm_survival_creator`. |
| **Job** | A user-made race, LTS, capture, deathmatch or survival. |
| **Global** | A slot in the scripts' shared global array, written `Global_N`. |
| **Local** | A variable on one script thread's stack, written `Local_N` / `fLocal_N` / `uLocal_N` by the decompiler. |
| **`f_N`** | Field N of a struct, N slots further. |
| **Stride / `NEXT`** | Size of one array element in slots. |
| **Offset** | In this project, usually a global/local index or field from `offsets.ini`, not a byte offset. |
| **AOB** | "Array of bytes": a byte pattern with wildcards used to find code or data in the game module. |
| **YSC** | GTA's compiled script format (bytecode). |
| **scrProgram** | The engine's in-memory object for a loaded script, holding its bytecode pages. |
| **scrpatch** | A bytecode patch Xenvious applies to a creator script at runtime. |
| **Custom function** | A small bytecode function Xenvious injects into a creator script. |
| **Worker** | The creator script's main state struct (`fLocal_...`), including the state machine (`f_565`) used for rebuild. |
| **Rebuild** | Forcing the creator to delete and re-create all placed entities from the job data. |
| **Prop / dynamic prop** | Static placed object vs. object with physics. Separate arrays with different strides. |
| **Template** | A saved group of props that can be placed as one. |
| **Rockstar cloud / UGC** | Where published jobs and their JSON live. |
| **ysc-global-updater** | The Python toolchain that regenerates offsets and patches for a new game build. |
| **mry** | The memory library used to read and write the game process. |
