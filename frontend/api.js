const API_URL = 'http://localhost:8080';

// --- GESTÃO DE SESSÃO ---
function guardarSessao(dados) {
    const token = dados.token || dados.Token;
    sessionStorage.setItem('jwt_token', token);
    const payload = JSON.parse(atob(token.split('.')[1]));
    const role = payload["role"] || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || "User";
    sessionStorage.setItem('user_role', role);
}

function obterToken() { return sessionStorage.getItem('jwt_token'); }
function obterRole() { return sessionStorage.getItem('user_role'); }
function estaLogado() { return obterToken() !== null; }
function limparToken() { sessionStorage.clear(); window.location.href = 'index.html'; }

// --- LIMPEZA DE MOCKS (Remove as barras/aspas do Mountebank) ---
function limparDadosMock(resultado) {
    if (resultado && resultado.mensagem && typeof resultado.mensagem === 'string') {
        try { return JSON.parse(resultado.mensagem); } catch (e) { return resultado.mensagem; }
    }
    return resultado;
}

// --- CHAMADA BASE ---
async function chamarAPI(endpoint, metodo = 'GET', corpo = null, comAuth = true) {
    const headers = { 'Content-Type': 'application/json' };
    if (comAuth) {
        const token = obterToken();
        if (!token) { limparToken(); return null; }
        headers['Authorization'] = `Bearer ${token}`;
    }
    const opcoes = { method: metodo, headers };
    if (corpo) opcoes.body = JSON.stringify(corpo);

    try {
        const resposta = await fetch(`${API_URL}${endpoint}`, opcoes);
        if (resposta.status === 401) { limparToken(); return null; }
        if (resposta.status === 503) return { erro: "circuit_breaker" };
        if (resposta.status === 204) return { sucesso: true };
        const dados = await resposta.json();
        return endpoint.includes('imposter') ? limparDadosMock(dados) : dados;
    } catch (erro) { return null; }
}

// --- FUNÇÕES DE NEGÓCIO ---
async function login(email, password) {
    const res = await chamarAPI('/api/auth/login', 'POST', { Email: email, Password: password }, false);
    if (res && (res.token || res.Token)) { guardarSessao(res); return { sucesso: true }; }
    return { sucesso: false, mensagem: res?.mensagem || 'Falha no login' };
}
async function registar(nome, email, password) { return await chamarAPI('/api/auth/register', 'POST', { Nome: nome, Email: email, Password: password }, false); }

// CRUD Produtos
async function listarProdutos() { return await chamarAPI('/api/produtos'); }
async function criarProduto(p) { return await chamarAPI('/api/produtos', 'POST', p); }
async function apagarProduto(id) { return await chamarAPI(`/api/produtos/${id}`, 'DELETE'); }

// CRUD Utilizadores (Novo: Requisito do Enunciado)
async function listarUtilizadores() { return await chamarAPI('/api/utilizadores'); }
async function apagarUtilizador(id) { return await chamarAPI(`/api/utilizadores/${id}`, 'DELETE'); }

// Mocks (Inventário e Pagamentos)
async function consultarInventario(sku) { return await chamarAPI(`/api/imposter/inventory/${sku}`); }
async function processarPagamento(dados) { return await chamarAPI('/api/imposter/payments', 'POST', dados); }