// Services/Payment/PaymentService.cs
using DigitalProject.Domain;
using DigitalProject.Exceptions;
using DigitalProject.Interface.Orders;
using DigitalProject.Interface.Payment;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Services.Payment
{
    public class PaymentService : IPaymentServie
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;  

        public PaymentService(
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository)  
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;  
        }

        // ── 舊的付款入口（保留）──
        public async Task<PaymentResponse> PayAsync(Guid userId, PaymentRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new AppException("訂單不存在", 404);
            if (order.UserId != userId)
                throw new AppException("無權限付款此訂單", 403);
            if (order.Status != OrderStatus.Pending)
                throw new AppException("此訂單無法付款");

            return request.Provider switch
            {
                PaymentProvider.CreditCard => await ProcessCreditCardAsync(request, order),
                PaymentProvider.CVS => await ProcessCVSAsync(request, order),
                _ => throw new AppException("不支援的付款方式")
            };
        }

        // ── 新的結帳入口（重構）──
        public async Task<CheckoutResponse> CheckoutAsync(Guid userId, CheckoutRequest request)
        {
            // 1. 查詢商品
            var products = (await _productRepository.GetByIdsAsync(request.ProductIds)).ToList();
            if (products.Count == 0)
                throw new AppException("找不到任何有效商品", 404);

            var totalAmount = products.Sum(p => p.Price);

            return request.Provider switch
            {
                PaymentProvider.CreditCard => await ProcessCreditCardCheckoutAsync(
                    userId, request, products, totalAmount),
                PaymentProvider.CVS => await ProcessCVSCheckoutAsync(
                    userId, products, totalAmount),
                _ => throw new AppException("不支援的付款方式")
            };
        }

        // ── 信用卡結帳（付款成功才建立訂單）──
        private async Task<CheckoutResponse> ProcessCreditCardCheckoutAsync(
            Guid userId,
            CheckoutRequest request,
            List<ProductResponse> products,
            decimal totalAmount)
        {
            // 1. 驗證信用卡
            if (string.IsNullOrEmpty(request.CardNumber) ||
                string.IsNullOrEmpty(request.CardHolder) ||
                string.IsNullOrEmpty(request.ExpiryDate) ||
                string.IsNullOrEmpty(request.Cvv))
                throw new AppException("請填寫完整信用卡資訊");

            ValidateCreditCardCheckout(request);

            // 2. 模擬付款（0000 結尾 → 失敗）
            var isSuccess = !request.CardNumber.Replace(" ", "").EndsWith("0000");
            if (!isSuccess)
                throw new AppException("信用卡付款失敗，請確認卡片資訊", 402);

            // 3. 付款成功才建立訂單
            var order = await CreateOrderWithItemsAsync(userId, products, totalAmount);

            // 4. 建立付款記錄
            var payment = new Models.Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Provider = PaymentProvider.CreditCard,
                TransactionId = "TXN-" + Guid.NewGuid().ToString("N")[..12].ToUpper(),
                Amount = totalAmount,
                Status = PaymentStatus.Paid,
                PaidAt = DateTime.UtcNow,
                IsVoid = false,
            };
            await _paymentRepository.CreateAsync(payment);

            // 5. 更新訂單狀態為 Paid
            await _orderRepository.UpdateStatusAsync(order.Id, OrderStatus.Paid);

            return new CheckoutResponse
            {
                OrderNo = order.OrderNo,
                OrderId = order.Id,
                Amount = totalAmount,
                Provider = PaymentProvider.CreditCard.ToString(),
                Status = "已付款"
            };
        }

        // ── 超商結帳（先建立訂單再給繳費代碼）──
        private async Task<CheckoutResponse> ProcessCVSCheckoutAsync(
            Guid userId,
            List<ProductResponse> products,
            decimal totalAmount)
        {
            // 1. 建立訂單（Pending）
            var order = await CreateOrderWithItemsAsync(userId, products, totalAmount);

            // 2. 建立付款記錄
            var payment = new Models.Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Provider = PaymentProvider.CVS,
                TransactionId = "CVS-" + Guid.NewGuid().ToString("N")[..12].ToUpper(),
                Amount = totalAmount,
                Status = PaymentStatus.Pending,
                PaymentCode = GenerateCVSCode(),
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IsVoid = false,
            };
            await _paymentRepository.CreateAsync(payment);

            return new CheckoutResponse
            {
                OrderNo = order.OrderNo,
                OrderId = order.Id,
                Amount = totalAmount,
                Provider = PaymentProvider.CVS.ToString(),
                Status = "待付款",
                PaymentCode = payment.PaymentCode,
                ExpiresAt = payment.ExpiresAt
            };
        }

        // ── 建立訂單共用方法 ──
        private async Task<Order> CreateOrderWithItemsAsync(
            Guid userId,
            List<ProductResponse> products,
            decimal totalAmount)
        {
            var items = products.Select(p => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = p.Id,
                ProductName = p.Name,
                UnitPrice = p.Price,
                Quantity = 1,
                SubTotal = p.Price
            }).ToList();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderNo = "DV-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderItems = items,
            };

            await _orderRepository.CreateAsync(order);
            return order;
        }

        // ── 舊的信用卡付款（保留）──
        private async Task<PaymentResponse> ProcessCreditCardAsync(
            PaymentRequest request, Order order)
        {
            if (string.IsNullOrEmpty(request.CardNumber) ||
                string.IsNullOrEmpty(request.CardHolder) ||
                string.IsNullOrEmpty(request.ExpiryDate) ||
                string.IsNullOrEmpty(request.Cvv))
                throw new AppException("請填寫完整信用卡資訊");

            ValidateCreditCard(request);

            var isSuccess = !request.CardNumber.Replace(" ", "").EndsWith("0000");

            var payment = new Models.Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Provider = PaymentProvider.CreditCard,
                TransactionId = "TXN-" + Guid.NewGuid().ToString("N")[..12].ToUpper(),
                Amount = order.TotalAmount,
                Status = isSuccess ? PaymentStatus.Paid : PaymentStatus.Failed,
                PaidAt = isSuccess ? DateTime.UtcNow : null,
                IsVoid = false,
            };

            await _paymentRepository.CreateAsync(payment);

            if (isSuccess)
                await _orderRepository.UpdateStatusAsync(order.Id, OrderStatus.Paid);

            return MapToResponse(payment, order.OrderNo);
        }

        // ── 舊的超商付款（保留）──
        private async Task<PaymentResponse> ProcessCVSAsync(
            PaymentRequest request, Order order)
        {
            var payment = new Models.Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Provider = PaymentProvider.CVS,
                TransactionId = "CVS-" + Guid.NewGuid().ToString("N")[..12].ToUpper(),
                Amount = order.TotalAmount,
                Status = PaymentStatus.Pending,
                PaymentCode = GenerateCVSCode(),
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IsVoid = false,
            };

            await _paymentRepository.CreateAsync(payment);
            return MapToResponse(payment, order.OrderNo);
        }

        // ── 超商繳費確認 ──
        public async Task<PaymentResponse> ConfirmCVSPaymentAsync(Guid paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);

            if (payment == null)
                throw new AppException("付款記錄不存在", 404);
            if (payment.Provider != PaymentProvider.CVS)
                throw new AppException("此付款不是超商繳費");
            if (payment.Status != PaymentStatus.Pending)
                throw new AppException("此付款已處理");
            if (payment.ExpiresAt < DateTime.UtcNow)
                throw new AppException("繳費期限已過");

            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            await _orderRepository.UpdateStatusAsync(
                payment.OrderId, OrderStatus.Paid);

            return MapToResponse(payment, payment.Order?.OrderNo ?? string.Empty);
        }

        // ── 取得訂單付款紀錄 ──
        public async Task<List<PaymentResponse>> GetByOrderIdAsync(Guid orderId)
        {
            var payments = await _paymentRepository.GetByOrderIdAsync(orderId);
            return payments.Select(p =>
                MapToResponse(p, p.Order?.OrderNo ?? string.Empty)).ToList();
        }

        // ── 查所有付款（後台）──
        public async Task<List<PaymentResponse>> GetAllAsync()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return payments.Select(p =>
                MapToResponse(p, p.Order?.OrderNo ?? string.Empty)).ToList();
        }

        // ── 作廢付款（管理員）──
        public async Task<PaymentResponse> VoidAsync(
            Guid adminUserId, Guid paymentId, string reason)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                throw new AppException("付款記錄不存在", 404);
            if (payment.IsVoid)
                throw new AppException("此付款已作廢");

            payment.IsVoid = true;
            payment.VoidByUserId = adminUserId;
            payment.VoidReason = reason;
            payment.VoidAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);
            return MapToResponse(payment, payment.Order?.OrderNo ?? string.Empty);
        }

        // ── 信用卡格式驗證（舊 PayAsync 用）──
        private static void ValidateCreditCard(PaymentRequest request)
        {
            var cardNumber = request.CardNumber!.Replace(" ", "");
            if (cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
                throw new AppException("信用卡卡號格式錯誤，需為 16 位數字");

            if (request.Cvv!.Length < 3 || request.Cvv.Length > 4
                || !request.Cvv.All(char.IsDigit))
                throw new AppException("CVV 格式錯誤");

            var parts = request.ExpiryDate!.Split('/');
            if (parts.Length != 2
                || !int.TryParse(parts[0], out var month)
                || !int.TryParse(parts[1], out var year)
                || month < 1 || month > 12)
                throw new AppException("到期日格式錯誤，請使用 MM/YY");

            var expiry = new DateTime(2000 + year, month, 1).AddMonths(1);
            if (expiry < DateTime.UtcNow)
                throw new AppException("信用卡已過期");
        }

        // ── 信用卡格式驗證（新 CheckoutAsync 用）──
        private static void ValidateCreditCardCheckout(CheckoutRequest request)
        {
            var cardNumber = request.CardNumber!.Replace(" ", "");
            if (cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
                throw new AppException("信用卡卡號格式錯誤，需為 16 位數字");

            if (request.Cvv!.Length < 3 || request.Cvv.Length > 4
                || !request.Cvv.All(char.IsDigit))
                throw new AppException("CVV 格式錯誤");

            var parts = request.ExpiryDate!.Split('/');
            if (parts.Length != 2
                || !int.TryParse(parts[0], out var month)
                || !int.TryParse(parts[1], out var year)
                || month < 1 || month > 12)
                throw new AppException("到期日格式錯誤，請使用 MM/YY");

            var expiry = new DateTime(2000 + year, month, 1).AddMonths(1);
            if (expiry < DateTime.UtcNow)
                throw new AppException("信用卡已過期");
        }

        // ── 產生超商繳費代碼 ──
        private static string GenerateCVSCode()
        {
            var random = new Random();
            return string.Concat(
                Enumerable.Range(0, 14)
                    .Select(_ => random.Next(0, 10).ToString()));
        }

        // ── MapToResponse ──
        private static PaymentResponse MapToResponse(Models.Payment p, string orderNo) => new()
        {
            Id = p.Id,
            OrderId = p.OrderId,
            OrderNo = orderNo,
            Amount = p.Amount,
            TransactionId = p.TransactionId,
            Status = p.Status,
            Provider = p.Provider.ToString(),
            PaidAt = p.PaidAt,
            IsVoid = p.IsVoid,
            VoidReason = p.VoidReason,
            VoidAt = p.VoidAt,
            PaymentCode = p.PaymentCode,
            ExpiresAt = p.ExpiresAt,
            UserEmail = p.Order?.User?.Email,
            UserDisplayName = p.Order?.User?.DisplayName,
        };
    }
}