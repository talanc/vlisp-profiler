(setq i 3)
(while (>= (setq i (1- i)) 0)
  (setq p1 (list 0 (* i 30))
	p2 (list 200 (* i 30))
	)
  (command "pline" p1 p2 "")
  )
