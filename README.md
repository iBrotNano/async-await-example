# Async Await

## Was heißt asynchron?

Was heißt synchron? Synchron heißt Code wird Schritt für Schritt ausgeführt. Asynchron heißt also, dass dies nicht der Fall ist. Die Ausführung einer Methode kann asynchron gestartet werden, während die ausführende Methode weiterläuft. Das ist es auch schon.

```c#
private async Task DoStuffAsync()
{
    _ = DoSomeStuffAsync();
    DoSomeOtherStuff();
}
```

`DoSomeStuffAsync()` wird gestartet und beginnt seine Arbeit während die Methode weiterläuft und `DoSomeOtherStuff()` ausgeführt wird.

Auf diese Weise kann man z.B. Anwendungen schreiben, deren UI bedienbar bleibt, auch wenn im Hintergrund komplexe Berechnungen durchgeführt werden.

## Task und Task<T>

Die Implementierung in .NET ist eine Implementierung des **Promise Model of Concurrency** pattern. Wir bekommen das Versprechen, dass Arbeit erledigt wird und können über eine API damit arbeiten.

`Task` ist eine Operation ohne einen Rückgabewert.

`Task<T>` ist eine Operation welche einen Rückgabewert des Typs `T` hat.

## Async - Await

`async` ist ein Schlüsselwort mit der man eine Methode kennzeichnen muss, die asynchronen Code ausführen darf. Mehr macht dieses Schlüsselwort nicht. Sobald man `async` in einer Methodensignatur verwendet, weiß der Compiler, dass man innerhalb der Methode mit `await` auf das Ergebnis einer Operation warten kann.

Klarer wird es wenn man sich das Verhalten tatsächlich einmal anschaut. Ich habe dazu eine Beispielprogramm geschrieben, mit dem man am Beispiel einer Retry-Methode das Verhalten beobachten kann.

In der Beispielanwendung gibt es mehrere Buttons, die die einzelnen Fälle starten.

Fall 1 führt eine asynchrone Methode ohne `await` aus. Die aufrufende Methode läuft weiter ohne dass auf das Beenden gewartet wird. Die UI bleibt bedienbar.

Dieser Fall ist geeignet, wenn das Ergebnis der asynchronen Methode nicht wichtig ist.

```c#
WriteToTextbox("Case 1");

// Case 1: Asynchrone Methode ohne await. Die Methode wird
// ausgeführt, es wird aber nicht darauf gewartet, dass die Methode
// beendet wird. Die UI bleibt responsiv.
Retry.RetryActionAsync(() => WriteToTextbox($"Executed and nothing is returned.")
  , TimeSpan.FromSeconds(1)
  , _cancellationToken
  , _progress);
```
Visual Studio zeigt die Methode unterstrichen an. Es handelt sich um korrekte Syntax, aber es wäre besser dem Compiler mitzuteilen, was wir mit dem Ergebnis machen wollen.

Das nächste Beispiel zeigt wie wir das Ergebnis explizit verwerfen.

```c#
WriteToTextbox("Case 2");

// Case 2: Asynchrone Methode ohne await. Die Methode wird
// ausgeführt, es wird aber nicht darauf gewartet, dass die Methode
// beendet wird. Die Methode unterscheidet sich von der Methode oben
// nur dadurch, dass das Resultat der Operation ins Nichts gesendet
// wird. _ = ist die Syntax dafür. Der Compiler weißt, dass mich das
// Ergebnis der gestarteten Methode nicht interessiert und die grüne
// Wellenlinie aus dem ersten Beispiel ist verschwunden.
_ = Retry.RetryActionAsync(() => WriteToTextbox($"Executed and nothing is returned.")
    , TimeSpan.FromSeconds(1)
    , _cancellationToken
    , _progress);
```

Im nächsten Beispiel geht es ans Eingemachte. Es wird mit `await` auf das Ende der asynchronen Methode gewartet. Die aufrufende Methode wird erst weiter ausgeführt, wenn die Arbeit der asynchronen Methode getan ist.

Die UI bleibt aber trotzdem bedienbar. Warum ist das so? Die Methode ist asynchron. Das heißt sie blockiert nicht den aktuellen Thread. In diesem Fall den UI-Thread.

Der Compiler erzeugt aus `await` zwei Methoden. Eine enthält den Code bis zum `await` mit einem Callback zur zweiten Methode, die den Code nach dem `await` enthält.

```c#
WriteToTextbox("Case 3");

try
{
    // Case 3: Asynchrone Methode mit await. Die Methode wird
    // ausgeführt und es wird auf deren Beendigung gewartet. Die
    // Methode läuft erst weiter, wenn das Polling beendet wurde.
    // Die UI bleibt aber responsiv. Das ist der Unterschied zur
    // synchronen Ausführung.
    await Retry.RetryActionAsync(() => WriteToTextbox($"Executed and nothing is returned.")
        , TimeSpan.FromSeconds(1)
        , _cancellationToken
        , _progress);
}
catch { }
```

## Task synchron ausführen

In manchen Fällen kann es vorkommen, dass eine API nur eine asynchrone Methode bereit stellt. Man kann diese dann synchron ausführen. Fall 4 zeigt wie dies geht:

```c#
WriteToTextbox("Case 4");

// Case 4: Code wird synchron ausgeführt auch.
Retry.RetryActionAsync(() => Console.WriteLine($"Executed and nothing is returned.")
    , TimeSpan.FromSeconds(1)
    , _cancellationToken
    , _progress)
    .GetAwaiter()
    .GetResult();
```

Mit `.GetAwaiter().GetResult()` kann das Ergebnis abgewartet werden. `async` ist in der Signatur der aufrufenden Methode nicht nötig.

## Wann macht asynchrone Programmierung Sinn?

Grob gibt es zwei Fälle in denen asynchrone Programmierung Sinn macht. Wenn eine Methode lange läuft, weil I/O-Daten verarbeitet werden oder wenn eine komplexe Berechnung von der CPU ausgeführt wird.

Microsoft beschreibt die sehr ausführlich unter https://docs.microsoft.com/en-us/dotnet/standard/async-in-depth. Ich mag hier nicht nochmal alles vorkauen. Deshalb ist hier nur eine kurze Zusammenfassung.

### I/O

Im I/O-Fall arbeitet die CPU nur Kurz in der Zeit in der alles vorbereitet wird um die Operation durchzuführen. Danach wird die Kontrolle an das OS abgegeben. Die Anwendung führt dann nichts mehr aus, sondern wartet nur noch auf das Ergebnis vom OS. Einen `Thread` zu blockieren und nichts auszuführen macht keinen Sinn.

### CPU-intensive Aufgaben

CPU-intensive Aufgaben können auf einem anderen Thread ausgeführt werden. Dies ist wichtig, da `Task`s defaultmäßig auf dem `Thread` des Aufrufers ausgeführt werden. Der einzige Unterschied zur synchronen Ausführung ist nur, dass der `Thread` nicht blockiert wird.

Mit `Task.Run()` wird ein `Task` an den `ThreadPool` übergeben. Der Code wird dann tatsächlich parallel abgearbeitet.

### Wann wie implementieren?

Microsoft empfiehlt folgendes Vorgehen:

Wenn es sich um I/O-Methoden handelt sollte man nur eine asynchrone Implementierung schreiben.

Wenn es reine Berechnungen sind sollte man nur eine synchrone Implementierung schreiben. Benutzer der Methode können dann selbst entscheiden ob sie mit `Task.Run()` diesen Code parallel abarbeiten lassen.

Viele Methoden enthalten beide Arten von Logik. Sie sollten wie I/O-Methoden asynchron implementiert werden.