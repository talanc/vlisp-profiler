(defun example ( / osmode i n)
(progn (prof:in "3") (prof:out (progn (progn (prof:in "4") (prof:out (setq osmode (progn (prof:in "5") (prof:out (getvar "osmode")))
i 0 n
100))) (progn (prof:in "6") (prof:out (while (progn (prof:in "7") (prof:out (< i n))) (progn (prof:in "8") (prof:out (command "pline" (progn (prof:in "9") (prof:out (list (progn (prof:in "10") (prof:out (* i 25))) 0)))
(progn (prof:in "11") (prof:out (list (progn (prof:in "12") (prof:out (* i 25))) 2000))) "")))
(progn (prof:in "13") (prof:out (command "pline" (progn (prof:in "14") (prof:out (list 0 (progn (prof:in "15") (prof:out (* i 25))))))
(progn (prof:in "16") (prof:out (list 2000 (progn (prof:in "17") (prof:out (* i 25)))))) ""))) (progn (prof:in "18") (prof:out (setq i (progn (prof:in "19") (prof:out (1+ i)))))))))
(progn (prof:in "20") (prof:out (setvar "osmode" osmode))) (progn (prof:in "21") (prof:out (princ))))))
)