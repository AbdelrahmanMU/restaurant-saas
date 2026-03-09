import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';

export interface OrderDto {
  id: string;
  number: string;
  status: string;
  type: string;
  tableNumber: string | null;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  constructor(private api: ApiClientService) {}

  getOrders(): Observable<OrderDto[]> {
    return this.api.get<OrderDto[]>('/orders');
  }
}
