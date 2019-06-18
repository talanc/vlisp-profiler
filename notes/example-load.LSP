(setq osmode (getvar "osmode")
      i	     0
      n	     100
)
(setvar "osmode" 0)

(while (<= i n)
  (command "pline"
	   (list (* i 25) 0)
	   (list (* i 25) 2500)
	   ""
  )
  (command "pline"
	   (list 0 (* i 25))
	   (list 2500 (* i 25))
	   ""
  )

  (setq i (1+ i))
)

(setvar "osmode" osmode)

(princ)