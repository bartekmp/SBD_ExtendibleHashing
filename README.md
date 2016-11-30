# SBD_ExtendibleHashing

Extendible hashing algorithm in C#. Records are containing an integer key and quadratic equation coefficients **A**, **B**, **C** as a value (i.e. `A*x^2+B*x+C = 0`).


Usage:

•	`N main_file_path records_per_page` – creates new structure with given parameters (_main file path_ and _number of records per directory page_)

•	`A key A B C` – adds a record with a key _key_ and coefficients: **A**, **B** and **C**

•	`A key` – adds a record with a given key _key_ and random coefficients

•	`U key A B C` – updates record with a key _key_ and new coefficients

•	`R key` – removes record with a given key _key_

•	`G key` – get a record with key _key_ (output format: `[pseudokey] key => coeffs sum`)

•	`PD` – print whole directory

•	`PF` – print main file, with records

•	`S` – print _reads from disk_ and _writes to disk_ on standard output

