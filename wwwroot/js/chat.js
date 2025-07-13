"use strict";

let connection = null;
 
async function startChat(token) {

    document.getElementById("login").style.display = "none";
    document.getElementById("chat").style.display = "block";
        
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub?access_token=" + token)
        .build()
    
    connection.onclose(() => {
        console.warn("Desconectado");
    });

    //Disable the send button until connection is established.
    document.getElementById("sendButton").disabled = true;

    connection.on("ReceiveMessage", function (user, message) {

        var li = document.createElement("li");
        document.getElementById("messagesList").appendChild(li);
        li.textContent = `${user}:${message}`;
    });

    connection.on("ReceiveGroupMessage", (user, message) => {
        const li = document.createElement("li");
        li.textContent = `[GRUPO] ${user}: ${message}`;
        document.getElementById("messagesList").appendChild(li);
    });

    connection.on("UpdateUserList", users => {
        const listContainer = document.getElementById("userList");
        listContainer.innerHTML = "";

        users.forEach(user => {
            const li = document.createElement("li");
            li.innerHTML = `
                <label>
                    <input type="radio" name="destino" value="${user}" />
                    ${user}
                </label>
            `;
            listContainer.appendChild(li);
        });
    });

    connection.start().then(function () {
        document.getElementById("sendButton").disabled = false;
        carregarUsuarios();
    }).catch(function (err) {
        return console.error(err.toString());
    });

    // setInterval(() => {
    //     console.log("Estado da conexão:", connection.state);
    //     if (connection.state !== signalR.HubConnectionState.Connected) {
    //         console.warn("Tentando reconectar...");
    //         connection.start().catch(err => console.error("Erro ao reconectar:", err));
    //     }else{
    //         connection.invoke("GetConnectedUsers").then(users => {
    //             document.getElementById("usersConnectList").value = users;
    //         }); 
    //     }
    // }, 5000);
    
}

document.getElementById("sendButton").addEventListener("click", function (event) {
   // var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessageToAll", message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("joinGroup").addEventListener("click", function (event) {
    const group = document.getElementById("groupInput").value;
    connection.invoke("JoinGroup", group);
    event.preventDefault();
});

document.getElementById("sendToGroup").addEventListener("click", function (event) {
    const group = document.getElementById("groupInput").value;
   // const user = document.getElementById("userInput").value;
    const message = document.getElementById("messageInput").value;
    connection.invoke("SendMessageToGroup", group, message);
    event.preventDefault();
});

document.getElementById("sendPrivateMessage").addEventListener("click", function (event) {
    const toUser =  document.querySelector('input[name="destino"]:checked').value;
    var message = document.getElementById("messageInput").value;
    console.log(toUser, message)
    if (toUser && message) {
        connection.invoke("SendMessageToUser", toUser, message);
    }
    event.preventDefault();
});


document.getElementById("sendAgentIa").addEventListener("click", function (event) {
    var message = document.getElementById("messageInput").value;
    if (message) {
        connection.invoke("sendAgentIa", message);
    }
    event.preventDefault();
});

function logout() {
    localStorage.removeItem("jwt_token");
    localStorage.removeItem("userLogado");
    location.reload();  
}

async function reconnectIfTokenExists() {
    const token = localStorage.getItem("jwt_token");

    if (token) {
        console.log("Token encontrado. Conectando...");
        setUserLogado()
        await startChat(token);
    } else {
        console.warn("Nenhum token encontrado.");
    }
}

async function login() {
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;

    const response = await fetch("/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password })
    });

    if (response.ok) {
        const data = await response.json();
        localStorage.setItem("jwt_token", data.token);
        localStorage.setItem("userLogado", username);
        setUserLogado()
        startChat(data.token);
    } else {
        alert("Login inválido!");
    }
}

async function carregarUsuarios() {
    const users = await connection.invoke("GetConnectedUsers");
    const userLogado = localStorage.getItem("userLogado");
    const listContainer = document.getElementById("userList");
    listContainer.innerHTML = ""; // limpar lista anterior

    users.forEach(user => {
        const li = document.createElement("li");
        li.innerHTML = `
            <label>
                <input type="radio" name="destino" value="${user}" />
                ${user}
            </label>
        `;
        if(user != userLogado)
            listContainer.appendChild(li);
    });
}

function setUserLogado()
{
    const userLogado = localStorage.getItem("userLogado");
     const divUserLogado = document.getElementById("userLogado");
    divUserLogado.innerHTML =  `<strong>${userLogado}</strong>`  ; 
}

window.onload = () => {
    reconnectIfTokenExists();
};