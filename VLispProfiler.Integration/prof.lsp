(setq prof:file nil)
(setq prof:symbols nil)
(setq prof:indent "")

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
  (command "netload" "prof.dll") ;; contains prof:elapsed
  )

(defun prof:write-trace (trac)
  (write-line (strcat prof:indent
		      (car prof:symbols) "," trac "," (prof:elapsed)
		      )
    prof:file)
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

(defun c:prof ( / path func)
  (setq path (getfiled "Select LISP file" "" "lsp" 0))
  (if path
    (progn
      (setq func (getstring nil "function (or leave empty for load exec): "))
      (if (= func "")
	(progn
	  
	  (prof:in "1")
	  (prof:out (load path))
	  )
	)
      )
    )
  )
