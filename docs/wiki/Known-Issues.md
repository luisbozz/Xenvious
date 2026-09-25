# Known issues and open work

Good places to start. Check the issue tracker for the current state before
you pick one.

| Area | Issue |
|---|---|
| Rebuild | Deathmatch and Survival use a different worker state numbering; their rebuild is not verified, and the Deathmatch worker local is wrong on both editions. |
| Enhanced | The creator camera teleport does not find the fly-cam object. |
| Enhanced | The "Online version" shown in the app is empty (version pointer offset). |
| Enhanced | Limits other than the prop limit (300) are unknown. |
| Legacy | Precise templates still need the Legacy locals and menu ids. |
| Both | Time of day does not work. |
| Props | Moving a prop from static to dynamic has been reported to crash. |
| Copy jobs | Loading a job from Social Club could hang while loading the job data; verify after the latest fixes. |
| Cleanup | `_used_keys.txt` in the repository root is not used by anything. |
