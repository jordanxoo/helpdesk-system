# AWS CDK Infrastructure

Ten folder zawiera Infrastructure as Code dla AWS.

## TODO: Do implementacji

1. **VPC i Networking**
   - VPC z publicznymi i prywatnymi podsieciami
   - Internet Gateway i NAT Gateway
   - Security Groups dla serwisów

2. **ECS (Elastic Container Service)**
   - ECS Cluster
   - Task Definitions dla każdego mikroservisu
   - ECS Services z Auto Scaling

3. **Application Load Balancer**
   - ALB dla API Gateway
   - Target Groups
   - Listener Rules

4. **RDS PostgreSQL**
   - Multi-AZ RDS instance
   - Parameter Groups
   - Subnet Groups

5. **SQS i SNS**
   - Kolejki SQS dla messaging między serwisami
   - SNS Topics dla powiadomień

6. **S3**
   - Buckets dla logów
   - Buckets dla załączników

7. **CloudWatch**
   - Log Groups
   - Metryki i Alarmy

8. **IAM**
   - Role dla ECS Tasks
   - Policies dla dostępu do AWS Services

## Komendy

```bash
# Deploy using CloudFormation
aws cloudformation create-stack \
  --stack-name helpdesk-system \
  --template-body file://cloudformation-template.yaml \
  --parameters ParameterKey=Environment,ParameterValue=dev \
  --capabilities CAPABILITY_IAM
```
