import { inject, Injectable } from '@angular/core';
import { ApiHttpService } from './api-http.service';
import { ProductQueryRequest, ProductResponse, ProductUpsert } from '@core/models/product.model';
import { Observable } from 'rxjs';
import { PagedResult } from '@core/models/paged-result.model';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  api = inject(ApiHttpService);

  getAdvanced(query: ProductQueryRequest) : Observable<PagedResult<ProductResponse>>{
    return this.api.post('/products/getall', query);
  }  

  getById(id: number) : Observable<ProductResponse>{
    return this.api.get(`/products/${id}`);
  }

  create(data: ProductUpsert) : Observable<ProductResponse>{
    return this.api.post('/products', data);
  }

  update(id: number, data: ProductUpsert) : Observable<ProductResponse>{
    return this.api.put(`/products/${id}`, data);
  }

  delete(id: number) : Observable<void>{
    return this.api.delete(`/products/${id}`);
  }
}
