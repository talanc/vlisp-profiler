(setq prof:file nil)
(setq prof:symbols nil)
(setq prof:indent "")
(setq prof:exe "vlisp-profiler.exe")

(defun prof:get-exe ()
  (if (and prof:exe (findfile prof:exe))
    prof:exe
    (setq prof:exe (getfiled "Find vlisp-profiler.exe" "vlisp-profiler.exe" "exe" 0))
    )
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

(if (findfile "prof.dll")
  (command "netload" "prof.dll") ;; hopefully contains prof:elapsed
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
  (if (prof:get-exe) ;; sets prof:exe
    (progn
      (setq path (getfiled "Select LISP file" "" "lsp" 0))
      (if path
	(progn
	  (setq func-sym (read (getstring nil "function name (empty for load exec): ")))
	  
	  (setq args (strcat "profile -f " (prof:file-path path)))
	  (if func-sym
	    (setq args (strcat args " -s 1:Load 2:Run"))
	    (setq args (strcat args " -s 1:LoadRun"))
	    )
	  
	  (prof:prof path func-sym)
	  )
	)
      )
    )
  )

(defun prof:prof (path func-sym / func-sym-type path-prof path-trac args)
  (cond
    ((null func-sym)
     nil ;; OK
     )
    ((vl-symbolp func-sym)
     (setq func-sym-type (type (vl-symbol-value func-sym)))
     (if (/= func-sym-type 'SUBR 'USUBR)
       (progn
	 (princ "func-sym must be nil or a function symbol")
	 (exit)
	 )
       )
     )
    (t
     (princ "func-sym must be nil or a function symbol")
     (exit)
     )
    )
  
  (setq path-prof (strcat path ".prof.lsp")
	path-trac (strcat path ".traces.txt")
	)
  
  (setq args (strcat "profile -f " (prof:file-path path)))
  (if func-sym
    (setq args (strcat args " -s 1:Load 2:Run"))
    (setq args (strcat args " -s 1:LoadRun"))
    )
  
  (startapp (prof:get-exe) args)
  
  ;; TODO, figure out a way to automate this
  (getstring nil "type go!")
  
  (setq prof:file (open path-trac "w"))
  
  (if func-sym
    (progn
      ;; Load
      (prof:in "1")
      (load path-prof)
      (prof:out nil)
      
      ;; Run
      (prof:in "2")
      ((vl-symbol-value func-sym))
      (prof:out nil)
      )
    (progn
      ;; LoadRun
      (prof:in "1")
      (load path-prof)
      (prof:out nil)
      )
    )
  
  (if prof:file
    (progn
      (close prof:file)
      (setq prof:file nil)
      )
    )
  
  (setq args (strcat "view -f " (prof:file-path path)))
  (startapp (prof:get-exe) args)

  (princ)
  )
