## **Introducere**
- Ideea de baza a proiectului
- Motivatie (accent pe necesitatea folosirii unui algoritm inteligent) 

## **Problema stiintifica abordata**
- Descrierea (plastica si formala) a problemei stiintifice care trebuie rezolvata
- Adaugarea unui abstract grafic (o diagrama care sa reflecte pasii efectuati in aplicatie si rolul/ideea de baza a lor) - un exemplu gasiti [aici](https://ars.els-cdn.com/content/image/1-s2.0-S0010482519303014-fx1_lrg.jpg)

## **Metode existente de rezolvare a problemei (related work)**

|Id| Paper | Dataset | Algoritm | Performance |
|--|---|---------|-----------|----------|
|1| [Improved Handwriting Recognition System using Capsule Network](http://crc.nilai.edu.my/URC2021/images/docs/ID_9.pdf) |[EMNIST](https://www.nist.gov/itl/products-and-services/emnist-dataset)| CapsNet | Acc. 95%|
|2| [TextCaps : Handwritten Character Recognition with Very Small Datasets](https://arxiv.org/abs/1811.08278) |[EMNIST](https://www.nist.gov/itl/products-and-services/emnist-dataset)| CapsNet | Acc. 90%..99.75%|
|3|[CNN-RNN BASED HANDWRITTEN TEXT RECOGNITION](https://ictactjournals.in/paper/IJSC_Vol_12_Iss_1_Paper_1_2457_2463.pdf)|[IAM](https://fki.tic.heia-fr.ch/databases/iam-handwriting-database)| CNN, RNN, CTC | Acc. 98% |
|4|[Off-Line and Online Handwritten Character Recognition Using RNN-GRU Algorithm](https://www.ijraset.com/best-journal/offline-and-online-handwritten-character-recognition-using-rnngru-algorithm)|[IAM](https://fki.tic.heia-fr.ch/databases/iam-handwriting-database)|RNN-CNN, GRU|Acc. 94%-97%|
|5|[MSdocTr-Lite: A Lite Transformer for Full Page Multi-script Handwriting Recognition](https://arxiv.org/pdf/2303.13931.pdf)|[IAM](https://fki.tic.heia-fr.ch/databases/iam-handwriting-database), [RIMES](https://paperswithcode.com/dataset/rimes), [KHATT](https://khatt.ideas2serve.net/), [Esposalles](http://dag.cvc.uab.es/the-esposalles-database/)| ResNet, Transformer model | CER 6.4?|
|6|[Scan, Attend and Read: End-to-End Handwritten Paragraph Recognition with MDLSTM Attention](https://arxiv.org/pdf/1604.03286.pdf)|[IAM](https://fki.tic.heia-fr.ch/databases/iam-handwriting-database)|MDLSTM-RNN|CER ~10%|



- Sfat: pentru fiecare lucrare referita mentionati urmatoarele aspecte: datele pe care s-a lucrat, algoritmul cu care s-a lucrat, rezultatele obtinute

### CapsNets

Un Capsule Network este o arhitectura deep learning propusa de Geoffrey Hinton in 2017 pentru a depasi limitele de performanta care afecteaza retelele CNN. Retelele CapsNet se abat de la structura pe layere a retelelor neuronale obisnuite, favorizand o structura bazata pe incapsularea straturilor (nested layers). O retea CapsNet se dovedeste a produce o acuratete mai mare avand nevoie de mai putine eforturi de antrenare. Spre deosebire de CNN, aceasta conserva feature-urile importante care altfel ar fi fost posibil alterate de layere MaxPooling, si nu este afectata de transformarile afine ale obiectelor prezente in imagine. 

- Masura calitatii modelului: combinatii functii de loss L1 & DSSIM, L1 & BCE, MSE & DSSIM, MSE & BCE, BCE & DSSIM
- Implementare: https://github.com/vinojjayasundara/textcaps

### Multidimensional Recurrent Neural Networks (MDRNNs)

MDRNN este o imbunatatire a RNN/LSTM al carei scop este de a suporta procesarea datelor in mai multe dimensiuni (ex. imagini). Intr-un MDRNN, un neuron primeste input atat din stratul inferior, cat si de la neuroni de pe acelasi strat care au fost procesati anterior. Retelele MDRNN au scopul de a face modelul imun la distorsiuni locale ale imaginii (scrisul rotit/oblic, linii subtiri groase, stilul literelor). Un RNN poate analiza inputuri de dimensiuni mari, insa are putere mica de procesare.

- Masura calitatii modelului: Character Error Rate (CER) 
- Implementare: https://github.com/Sagar-modelling/Handwriting_Recognition_CRNN_LSTM

### Connectionist Temporal Classification (CTC)

CTC este un algoritm specializat pentru speech recognition/handwriting recognition care mapeaza intregul input catre un singur output (clasa sau text). Pentru o clasificare mai precisa, CTC foloseste mai multe clase decat au fost initial declarate ca output, alegand rezultatul in urma unei decizii probabilistice  asupra tuturor acestor clase. CTC foloseste un RNN pentru a calcula probabilitatile la fiecare pas/moment de timp si emite o tabla de incredere care include fiecare clasa (ex. litera/sunet) si probabilitatea corespunsatoare.

### Transformer models

"The language knowledge is embedded into the model itself, so there is no need for any additional post-processing steps using a language model. It’s also well-suited to predict outputs that are not part of the vocabulary."

- Masura calitatii modelului: Character Error Rate (CER) 

### Encoder-Decoder and Attention Networks


## **Metode efectiv folosite pentru rezolvarea problemei**

- Descrierea algoritmilor inteligenti folositi, a procesului de invatare/optimizare implicat, a metricilor folosite pentru aprecierea calitatii rezolvarii problemei

## **Rezultate experimentale obtinute**
- Descrierea seturilor de date folosite (sursa datelor, clasificarea/tipologia datelor)
- Metodologia experimentala (care sunt întrebările la care ar trebui să răspundă exeperimentele efectuate)  si parametrii algoritmilor
- Rezultatele obtinute (măsurile de performanţă calculate ca urmare a aplicării clor 2 algoritmi inteligenti pentru rezolvarea problemei
- Analiza statistica a rezultatelor obtinute

## **Concluzii si posibile imbunatatiri**

## **Referinte** (formatate, adica editate in acelasi stil)

- https://www.v7labs.com/blog/handwriting-recognition-guide
- http://crc.nilai.edu.my/URC2021/images/docs/ID_9.pdf
- https://arxiv.org/abs/1811.08278
- https://ictactjournals.in/paper/IJSC_Vol_12_Iss_1_Paper_1_2457_2463.pdf
- https://www.ijraset.com/best-journal/offline-and-online-handwritten-character-recognition-using-rnngru-algorithm
- https://arxiv.org/pdf/2303.13931.pdf
- https://arxiv.org/pdf/1604.03286.pdf