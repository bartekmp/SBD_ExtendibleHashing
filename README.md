# SBD_ExtendibleHashing

Extendible hashing algorithm in C#. Records are containing an integer key and quadratic equation coefficients **A**, **B**, **C** as a value (i.e. `A*x^2+B*x+C = 0`).


Usage:

•	`N main_file_path records_per_page` – creates new organization with given parameters

•	`A key A B C` – adds a record with key "key" and coefficients: A, B and C

•	`A key` – adds a record with given key and random coefficients

•	`U key A B C` – updates record with key "key" and new coefficients

•	`R key` – removes record with given key

•	`G key` – get record (format: [pseudokey] key => coeffs sum)

•	`PD` – print directory

•	`PF` – print main file, with records

•	`S` – print reads/writes from/on disk

