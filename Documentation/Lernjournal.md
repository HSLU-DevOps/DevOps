# Lernjournal / Lessons Learned

## Zusammenarbeit und Organisation

Die Zusammenarbeit im Team war eines der Highlights dieses Projekts. Wir haben während und nach den Vorlesungen
kontinuierlich am Projekt gearbeitet. Nach dem Unterricht haben wir uns abgesprochen, welche Features wir noch ergänzen
möchten, basierend auf unserer Motivation und den im Unterricht besprochenen Konzepten. Dieses flexible Vorgehen hat
nicht nur die Arbeit erleichtert, sondern auch die Freude am Projekt erhalten.

Leider war unser drittes Gruppenmitglied, Silvan, nicht aktiv und hat sich weder im Unterricht gezeigt noch gemeldet.
Nach Rücksprache mit dem Dozenten haben wir beschlossen, eine andere VM zu erhalten, damit wir trotzdem weiterarbeiten
können. Diese Entscheidung fiel früh im Semester und ermöglichte uns, effektiv als Zweierteam weiterzuarbeiten.

---

## Persönliche Beiträge

**Ilija's Beiträge:**

- **Todo-App:**

  Ich habe mich auf die Implementierung der Todo-App konzentriert, einschliesslich API, Datenbankmigrationen und
  Integration mit Feature Flags. Dank meiner Vorerfahrung mit .NET konnte ich mich auf die Kernlogik und Tests
  fokussieren.

- **Tests:**

  Die Unit- und Integrationstests waren ein wesentlicher Bestandteil meiner Arbeit. Diese stellten sicher, dass alle
  Features zuverlässig funktionierten und die Quality-Gates bestanden.

- **Health Checks:**

  Die Implementierung von Health Checks (Datenbankverbindung) war eine interessante Aufgabe, die die Stabilität der
  Anwendung erhöht hat.

- **CI (qualitygate.yml & codeql.yml):**

  Ich habe den Grossteil der GitHub Actions für den CI-Teil erstellt, da ich mit dem .NET CLI vertraut bin. Es war
  spannend, CI-Pipelines von Grund auf zu konfigurieren und so mehr Einblick in GitHub Actions zu gewinnen, als ich aus
  meiner Praxis kannte.

**Lorin's Beiträge:**

- **Infrastuktur und Docker Swarm:**

  Lorin hat die Services für Monitoring und Container-Orchestrierung konfiguriert. Seine Expertise mit Docker Swarm war
  essenziell, da wir uns bewusst gegen Kubernetes entschieden haben. Swarm war einfacher und zuverlässiger in unserem
  Setup und wurde durch Lorin's fundierte Kenntnisse effizient eingesetzt.

- **CD (Labservices.yml):**

  Das Deployment (CD) wurde hauptsächlich von Lorin umgesetzt. Die Konfiguration der Labservices-Ressourcen war eng mit
  Docker Swarm verbunden, und seine Erfahrung half, diese komplexe Aufgabe erfolgreich abzuschliessen.

- **Docker-Stack-Konfiguration (docker-stack.yml)**:

  Lorin war für die Erstellung und Optimierung des docker-stack.yml verantwortlich. Dieses File regelt das Deployment in
  der Produktionsumgebung und ermöglicht eine skalierbare Bereitstellung unserer Anwendung.

- **GitHub-Secrets**:

  Lorin hat die GitHub-Secrets korrekt eingerichtet, um eine sichere Übergabe von Umgebungsvariablen (wie
  Datenbank-Passwörtern und API-Schlüsseln) an unsere Pipelines zu gewährleisten. Diese Konfiguration war entscheidend
  für ein sicheres und zuverlässiges Deployment.

**Gemeinsame Aufgaben:**

- **Self-Hosted Runners:**

  Wir haben gemeinsam die GitHub Self-Hosted Runners auf einer Linux VM im Labservices-Netzwerk eingerichtet. Die
  Anleitung von GitHub war hilfreich, und obwohl es für mich (Ilija) neu war, konnte Lorin dank seiner Erfahrung zügig
  Probleme lösen und die Maschinen produktiv machen.

- **Docker Swarm Cluster:**

  Wir haben ebenfalls gemeinsam das Docker Swarm Cluster in Betrieb genommen. Durch die Dokumentation von Docker und
  meinen (Lorin) Docker Vorkenntnissen, funktionierte dies auf Anhieb ohne Probleme.

- **Dokumentation**

---

## Erfahrungen und Erkenntnisse

**Ilija's Lessons Learned:**

- **Docker Swarm:**

  Vor diesem Projekt hatte ich wenig Erfahrung mit Docker Swarm. Durch Lorin's Anleitung und die praktische Anwendung
  habe ich ein tiefes Verständnis für Swarm und seine Vorteile im Vergleich zu anderen Orchestrierungstools gewonnen.

- **GitHub Actions:**

  Die Arbeit an GitHub Actions hat mir gezeigt, wie flexibel und mächtig diese Tools sind. Es war eine grossartige
  Erfahrung, CI/CD-Pipelines von Grund auf zu erstellen, anstatt nur bestehende zu erweitern.

- **12-Factor-App-Prinzipien:**

  Diese Prinzipien waren besonders faszinierend, da sie viele Best Practices für die Entwicklung moderner, skalierbarer
  Anwendungen zusammenfassen. Die Integration dieser Prinzipien in unser Projekt war ein lohnender Prozess und stärkte
  unser Verständnis für hochwertige Softwareentwicklung.

**Lorin's Lessons Learned:**

- **.NET und Buildtools:**

  Für mich war die Arbeit mit .NET eine neue Herausforderung. Ich lernte die grundlegenden Buildtools und die
  Arbeitsweise des .NET-Ökosystems kennen, welche ich zuvor noch gar nicht kannte.

- **Docker Swarm Cluster:**

  Das Aufsetzen und der Betrieb von einem Docker Swarm Cluster war für mich eine sehr lehrreiche Erfahrung. Da ich
  Docker Swarm in meinem Arbeitsalltag lediglich auf einem Single-Node verwende, war es sehr interessant zu sehen, wie
  sich die Konzepte von Service Discovery, Load Balancing und Replikation in der Praxis umsetzen lassen.

- **Monitoring & Logging:**

  Die Inbetriebnahme von Prometheus, Grafana, Loki und Promtail in einem Docker Swarm Cluster war ebenfalls äusserst
  interessant. Dank der Service-Discovery von Prometheus und Promtail gestaltete sich die Konfiguration und Integration
  der einzelnen Komponenten einfacher als zunächst angenommen. Besonders faszinierend war es zu beobachten, wie
  Prometheus automatisch die verschiedenen Services im Cluster erkennt und deren Metriken erfasst. Gleichzeitig konnte
  Promtail die Logs der Container ohne manuelle Konfiguration einsammeln und an Loki weiterleiten.

---

## Persönliches Fazit

Dieses Modul war eines der coolsten Module des Semesters. Wir haben uns nicht nur auf den Unterricht gefreut, sondern
auch auf die Zusammenarbeit. Der Austausch mit den Dozenten und den anderen Studenten war lehrreich und bereichernd. Die
Diskussionen über die praktische Anwendbarkeit verschiedener Technologien und Ansätze waren von unschätzbarem Wert.

Durch diese Unterhaltungen konnten wir unser Projekt ständig verbessern und mit Real-World Solutions erweitern.
Besonders spannend war es zu sehen, wie die Konzepte wie die 12-Factor-App und DevOps-Philosophien nahtlos ineinander
greifen.

Der Austausch mit der CSS war sehr interessant. Erfolgsgeschichten motivieren die meisten, dass man nichts "dummes"
lernt. Die Klassendynamik war auch immer angenehm.

Cool fand ich noch die deklarativen Pipelines, den Tech-Stack/Environment von Herrn Christen. Es ist immer ein Flex
mit Tmux, k8s und sämtlichen anderen TUI's zu präsentieren.

BTW: Das ist eine Referenz zu diesem [Video](https://www.youtube.com/watch?v=r6tH55syq0o)

0:00 - 0:45

---

## Zusammenarbeit und Perspektive

Unsere Rollen im Team haben sich perfekt ergänzt. Während ich (Ilija) mich auf die technische Implementierung
konzentriert habe, brachte Lorin seine Infrastruktur- und Docker-Erfahrung ein. Es war beeindruckend zu sehen, wie
harmonisch und produktiv wir als Team arbeiteten. Dieses Projekt hat nicht nur unser technisches Wissen erweitert,
sondern auch unsere Zusammenarbeit als Entwickler gestärkt.
