import { useCallback, useEffect, useState } from 'react';

const emptyForm = { name: '', price: '' };

export default function App() {
  const [products, setProducts] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const loadProducts = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await fetch('/api/products');
      if (!response.ok) throw new Error(`API returned ${response.status}`);
      setProducts(await response.json());
    } catch (err) {
      setError(`Cannot load products. ${err.message}`);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadProducts(); }, [loadProducts]);

  async function addProduct(event) {
    event.preventDefault();
    setSaving(true);
    setError('');
    try {
      const response = await fetch('/api/products', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: form.name.trim(), price: Number(form.price) })
      });
      if (!response.ok) throw new Error(`API returned ${response.status}`);
      setForm(emptyForm);
      await loadProducts();
    } catch (err) {
      setError(`Cannot add product. ${err.message}`);
    } finally {
      setSaving(false);
    }
  }

  async function deleteProduct(id) {
    setError('');
    try {
      const response = await fetch(`/api/products/${id}`, { method: 'DELETE' });
      if (!response.ok) throw new Error(`API returned ${response.status}`);
      await loadProducts();
    } catch (err) {
      setError(`Cannot delete product. ${err.message}`);
    }
  }

  return (
    <main>
      <header className="hero">
        <div>
          <span className="badge">Docker practice project</span>
          <h1>Docker Shop</h1>
          <p>React frontend, ASP.NET Core API, PostgreSQL database and Redis cache.</p>
        </div>
        <button className="secondary" onClick={loadProducts}>Refresh</button>
      </header>

      <section className="panel">
        <h2>Add a product</h2>
        <form onSubmit={addProduct}>
          <label>
            Product name
            <input required maxLength="100" value={form.name}
              onChange={e => setForm({ ...form, name: e.target.value })}
              placeholder="For example, Monitor" />
          </label>
          <label>
            Price
            <input required min="0.01" step="0.01" type="number" value={form.price}
              onChange={e => setForm({ ...form, price: e.target.value })}
              placeholder="850.00" />
          </label>
          <button disabled={saving}>{saving ? 'Adding...' : 'Add product'}</button>
        </form>
      </section>

      {error && <div className="error" role="alert">{error}</div>}

      <section className="products">
        <div className="section-title">
          <h2>Products</h2>
          <span>{products.length} item{products.length === 1 ? '' : 's'}</span>
        </div>
        {loading ? <p className="status">Loading products...</p> : products.length === 0 ?
          <p className="status">No products found. Add the first product above.</p> :
          <div className="grid">
            {products.map(product => (
              <article className="card" key={product.id}>
                <div className="product-icon">{product.name.slice(0, 1).toUpperCase()}</div>
                <div className="card-content">
                  <h3>{product.name}</h3>
                  <p>AED {Number(product.price).toFixed(2)}</p>
                </div>
                <button className="danger" onClick={() => deleteProduct(product.id)}>Delete</button>
              </article>
            ))}
          </div>}
      </section>
    </main>
  );
}
