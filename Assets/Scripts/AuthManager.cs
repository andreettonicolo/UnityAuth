using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;

public class AuthManager : MonoBehaviour
{
    // Variabili Firebase
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;

    // Variabili per il login
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    // Variabili per la registrazione
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    void Awake()
    {
        // Verifica che tutte le dipendenze necessarie per Firebase siano presenti nel sistema
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Se sono disponibili, inizializza Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Impossibile risolvere tutte le dipendenze di Firebase: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Configurazione di Firebase Auth");
        // Imposta l'istanza di autenticazione
        auth = FirebaseAuth.DefaultInstance;
    }

    // Funzione per il pulsante di login
    public void LoginButton()
    {
        // Chiama la coroutine di login passando email e password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    // Funzione per il pulsante di registrazione
    public void RegisterButton()
    {
        // Chiama la coroutine di registrazione passando email, password e username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    private IEnumerator Login(string _email, string _password)
    {
        // Chiama la funzione di accesso di Firebase Auth passando email e password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        // Attendere il completamento del task
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            // Se ci sono errori, gestiscili
            Debug.LogWarning(message: $"Impossibile completare il task di accesso: {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Accesso non riuscito!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Email mancante";
                    break;
                case AuthError.MissingPassword:
                    message = "Password mancante";
                    break;
                case AuthError.WrongPassword:
                    message = "Password errata";
                    break;
                case AuthError.InvalidEmail:
                    message = "Email non valida";
                    break;
                case AuthError.UserNotFound:
                    message = "Account non trovato";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            // L'utente è ora connesso
            // Ottieni il risultato
            User = LoginTask.Result.User;
            Debug.LogFormat("Accesso eseguito con successo: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Accesso effettuato";
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            // Se il campo dell'username è vuoto, mostra un avviso
            warningRegisterText.text = "Username mancante";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            // Se la password non corrisponde, mostra un avviso
            warningRegisterText.text = "Password non corrispondenti!";
        }
        else
        {
            // Chiama la funzione di registrazione di Firebase Auth passando email e password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            // Attendere il completamento del task
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                // Se ci sono errori, gestiscili
                Debug.LogWarning(message: $"Impossibile completare il task di registrazione: {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Registrazione non riuscita!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Email mancante";
                        break;
                    case AuthError.MissingPassword:
                        message = "Password mancante";
                        break;
                    case AuthError.WeakPassword:
                        message = "Password debole";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email già in uso";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                // L'utente è stato creato
                // Ora ottieni il risultato
                User = RegisterTask.Result.User;

                if (User != null)
                {
                    // Crea un profilo utente e imposta l'username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    // Chiama la funzione di aggiornamento del profilo utente di Firebase Auth passando il profilo con l'username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    // Attendere il completamento del task
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        // Se ci sono errori, gestiscili
                        Debug.LogWarning(message: $"Impossibile completare il task di registrazione: {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Impostazione dell'username non riuscita!";
                    }
                    else
                    {
                        // L'username è stato impostato
                        // Ora torna alla schermata di accesso
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                    }
                }
            }
        }
    }
}
