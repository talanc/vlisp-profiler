(setq prof:file nil)
(setq prof:symbols nil)
(setq prof:indent "")

(setq prof:exe nil)
(setq prof:exe-name "vlisp-profiler.exe")
(setq prof:netload nil)

(defun prof:get-exe ( / reg ff)
  (if (null prof:exe)
    (progn
      ;; findfile
      (if (null prof:exe)
	(if (setq ff (findfile prof:exe-name))
	  (setq prof:exe ff)
	  )
	)
      
      ;; registry install location
      (if (null prof:exe)
	(if (setq reg (vl-registry-read "HKEY_CURRENT_USER\\Software\\VLispProfiler" "InstallLocation"))
	  (setq prof:exe (strcat reg "\\" prof:exe-name))
	  )
	)
      
      ;; manual lookup
      (if (null prof:exe)
	(setq prof:exe (getfiled "Find vlisp-profiler.exe" "vlisp-profiler.exe" "exe" 0))
	)
      
      (if (null prof:exe)
	(prof:exit (strcat "Could not find " prof:exe-name))
	)
      )
    )
  
  (if (null prof:netload)
    (progn
      (if (findfile "prof.dll")
	(command "netload" "prof.dll") ;; hopefully contains vlisp function named prof:elapsed
	)
      (setq prof:netload t)
      )
    )
  
  prof:exe
  )

(defun prof:exit (msg)
  (princ msg)
  (exit)
  )

(defun prof:wait-file-exists (path freq-ms num / left)
  (if (null (findfile path))
    (progn
      (setq left num)
      (while (> left 0)
	(setq left (1- left))
	
	(cond
	  ((findfile path)
	   (setq left 0)
	   )
	  ((= left 0)
	   (setq path nil)
	   )
	  (t
	   (command "delay" freq-ms)
	   )
	  )
	)
      )
    )
  path
  )

(defun prof:file-path (path)
  (strcat "\"" path "\"")
  )

(defun prof:str-pad-left (value digits char)
  (if (< (strlen value) digits)
    (strcat char (prof:str-pad-left value (1- digits) char))
    value
    )
  )

(defun prof:str-pad-right (value digits char)
  (if (< (strlen value) digits)
    (strcat (prof:str-pad-right value (1- digits) char) char)
    value
    )
  )

;; .NET TimeSpan.ToString
;; https://msdn.microsoft.com/en-us/library/1ecy8h51(v=vs.110).aspx
(defun prof:elapsed (/ time days hours minutes seconds milliseconds)
  (setq time (getvar "TDUSRTIMER")
	days (fix time)
	hours (fix (rem (* time 24) 24))
	minutes (fix (rem (* time 24 60) 60))
	seconds (fix (rem (* time 24 60 60) 60))
	milliseconds (fix (rem (* time 24 60 60 1000) 1000))
	)
  
  (strcat (if (> days 0) (strcat (rtos days) ".") "")
	  (prof:str-pad-left (rtos hours) 2 "0")
	  ":"
	  (prof:str-pad-left (rtos minutes) 2 "0")
	  ":"
	  (prof:str-pad-left (rtos seconds) 2 "0")
	  "."
	  (prof:str-pad-left (rtos milliseconds) 3 "0")
	  )
  )

(defun prof:write-trace (trac)
  (write-line (strcat prof:indent
		      (car prof:symbols) "," trac "," (prof:elapsed)
		      )
    prof:file)
  )

(defun prof:val (in value)
  (prof:out value)
  )

(defun prof:in (symbol)
  (setq prof:symbols (cons symbol prof:symbols))
  (prof:write-trace "In")
  (setq prof:indent (strcat prof:indent "\t"))
  nil
  )

(defun prof:out (value)
  (setq prof:indent (substr prof:indent 2))
  (prof:write-trace "Out")
  (setq prof:symbols (cdr prof:symbols))
  value
  )

(defun c:prof ( / path func-sym)
  (prof:get-exe) ;; sets prof:exe
  
  (setq path (getfiled "Select LISP file" "" "lsp" 0))
  (if (null path) (exit))
  
  (setq func-sym (read (getstring nil "function name (empty for load exec): ")))
  
  (if (not (or (null func-sym) (vl-symbolp func-sym)))
    (prof:exit "invalid function name")
    )
  
  (prof:prof path func-sym)
  )

(defun prof:prof (path func-sym / path-prof path-trac path-comp path-erro args-sym args err)
  ;; profile files
  (setq path-prof (strcat path ".prof.lsp")
	path-trac (strcat path ".traces.txt")
	path-comp (strcat path ".complete.txt")
	path-erro (strcat path ".error.txt")
	)

  ;; delete files
  (foreach value (list path-prof path-trac path-comp path-erro)
    (if (findfile value)
      (vl-file-delete value)
      )
    )

  ;; generate profile files
  (if func-sym
    (setq args-sym "-s 1:Load 2:Run")
    (setq args-sym "-s 1:LoadRun")
    )
  (setq args (strcat "profile" " "
		     "-f " (prof:file-path path) " "
		     args-sym " "
		     "--complete-file " (prof:file-path path-comp) " "
		     "--error-file " (prof:file-path path-erro)
		     ))
  (startapp (prof:get-exe) args)

  ;; wait 30 seconds for profile command to complete
  (setq err (prof:wait-file-exists path-comp 1000 30))
  (if (null err)
    (prof:exit "generate timeout")
    )

  ;; any errors with command?
  (if (findfile path-erro)
    (prof:exit "error with command, see error.txt for details")
    )
  
  ;; open trace file
  (setq prof:file (open path-trac "w"))
  
  ;; profile!
  (setq err (vl-catch-all-apply 'prof:go (list path-prof func-sym)))
  
  ;; close trace file
  (close prof:file)
  (setq prof:file nil)
  
  (cond
    ;; fatal error
    ((vl-catch-all-error-p err)
     (princ (strcat "fatal error occured: " (vl-catch-all-error-message err)))
     )
    ;; error
    ((= (type err) 'STR)
     (princ err)
     )
    ;; otherwise, view profile
    (t
     (setq args (strcat "view -f " (prof:file-path path) " --top 10 --pause-top"))
     (startapp (prof:get-exe) args)
     )
    )
  
  (princ)
  )

(defun prof:sym-is-sub (sym / sym-type)
  (setq sym-type (type (vl-symbol-value sym)))
  (or (= sym-type 'SUBR) (= sym-type 'USUBR))
  )

(defun prof:go (path-prof func-sym / ret err func-sym-type)
  (setq ret nil
	err nil
	)
  
  ;; 1:Load (or 1:LoadRun if func-sym is nil)
  (prof:in "1")
  (setq err (vl-catch-all-apply 'load (list path-prof)))
  (prof:out nil)
  
  (if (vl-catch-all-error-p err)
    (setq ret (strcat "error loading file: " (vl-catch-all-error-message err)))
    
    ;; else, no error
    (if func-sym
      (if (null (prof:sym-is-sub func-sym))
	(setq ret (strcat "function does not exist: " (vl-symbol-name func-sym)))
	
	;; 2:Run
	(progn
	  (prof:in "2")
	  (setq err (vl-catch-all-apply func-sym nil))
	  (if (vl-catch-all-error-p err)
	    (setq ret (strcat "error running function " (vl-symbol-name func-sym) ": " (vl-catch-all-error-message err)))
	    )
	  (prof:out nil)
	  )
	)
      )
    )
  
  ret
  )
